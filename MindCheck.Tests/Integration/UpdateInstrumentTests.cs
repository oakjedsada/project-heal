using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MindCheck.Tests.Integration;

// Covers UpdateInstrumentUseCase: editing basic info/questions/choices/scoring
// rules in place, and the two safety checks that must block a destructive
// edit rather than silently corrupting history — deleting a question/choice
// a real user already answered, and removing/renaming a scoring level an
// existing Result already references.
public sealed class UpdateInstrumentTests : IClassFixture<MindCheckApiFactory>
{
    private readonly MindCheckApiFactory _factory;
    private readonly HttpClient _client;

    public UpdateInstrumentTests(MindCheckApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LoginAsAdminAsync().Result);
    }

    [Fact]
    public async Task Update_BasicInfoAndInPlaceEdits_PreservesIdsAndPersists()
    {
        var created = await CreateInstrumentAsync();
        var questionId = created.GetProperty("questions")[0].GetProperty("questionId").GetInt32();
        var choiceId = created.GetProperty("questions")[0].GetProperty("choices")[0].GetProperty("choiceId").GetInt32();
        var instrumentId = created.GetProperty("instrumentId").GetInt32();

        var updateRequest = new
        {
            code = created.GetProperty("code").GetString(),
            name = "Renamed instrument",
            version = "2.0",
            source = "updated-source",
            isActive = true,
            questions = new object[]
            {
                new
                {
                    questionId,
                    text = "Edited question text",
                    orderNo = 1,
                    choices = new object[]
                    {
                        new { choiceId, label = "Edited label", score = 9, orderNo = 1 },
                        new { label = "Brand new choice", score = 10, orderNo = 2 },
                    },
                },
            },
            scoringRules = new[]
            {
                new { min = 0, max = 20, level = "UpdatedLevel", interpretation = "Updated interpretation", advice = "Updated advice" },
            },
        };

        var response = await _client.PutAsJsonAsync($"/api/admin/instruments/{instrumentId}", updateRequest);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Renamed instrument");
        var updatedQuestion = body.GetProperty("questions")[0];
        updatedQuestion.GetProperty("questionId").GetInt32().Should().Be(questionId, "editing in place must keep the same id");
        updatedQuestion.GetProperty("text").GetString().Should().Be("Edited question text");
        updatedQuestion.GetProperty("choices").GetArrayLength().Should().Be(2);
        updatedQuestion.GetProperty("choices")[0].GetProperty("choiceId").GetInt32().Should().Be(choiceId);
    }

    [Fact]
    public async Task Update_DeletingAnAnsweredQuestion_IsBlocked()
    {
        var created = await CreateInstrumentAsync();
        var instrumentId = created.GetProperty("instrumentId").GetInt32();
        var questionId = created.GetProperty("questions")[0].GetProperty("questionId").GetInt32();
        var choiceId = created.GetProperty("questions")[0].GetProperty("choices")[0].GetProperty("choiceId").GetInt32();

        await ActivateAndAnswerAsync(instrumentId, questionId, choiceId);

        // Replace the answered question with a different one entirely — the
        // original must be rejected as "in use," not silently dropped.
        var updateWithReplacementQuestion = new
        {
            code = created.GetProperty("code").GetString(),
            name = created.GetProperty("code").GetString(),
            version = "1.0",
            source = "integration-test",
            isActive = true,
            questions = new[]
            {
                new
                {
                    text = "A different, unanswered question",
                    orderNo = 1,
                    choices = new[]
                    {
                        new { label = "A", score = 0, orderNo = 1 },
                        new { label = "B", score = 1, orderNo = 2 },
                    },
                },
            },
            scoringRules = new[]
            {
                new { min = 0, max = 5, level = "RuntimeLevel", interpretation = "x", advice = "y" },
            },
        };

        var response = await _client.PutAsJsonAsync($"/api/admin/instruments/{instrumentId}", updateWithReplacementQuestion);

        response.IsSuccessStatusCode.Should().BeFalse("the original question already has a recorded answer");
        ((int)response.StatusCode).Should().Be(400);
    }

    [Fact]
    public async Task Update_RemovingAScoringLevelWithExistingResults_IsBlocked()
    {
        var created = await CreateInstrumentAsync();
        var instrumentId = created.GetProperty("instrumentId").GetInt32();
        var questionId = created.GetProperty("questions")[0].GetProperty("questionId").GetInt32();
        var choiceId = created.GetProperty("questions")[0].GetProperty("choices")[0].GetProperty("choiceId").GetInt32();

        await ActivateAndAnswerAsync(instrumentId, questionId, choiceId);

        var updateRequest = new
        {
            code = created.GetProperty("code").GetString(),
            name = created.GetProperty("code").GetString(),
            version = "1.0",
            source = "integration-test",
            isActive = true,
            questions = new[]
            {
                new
                {
                    questionId,
                    text = "Runtime-created question",
                    orderNo = 1,
                    choices = new[] { new { choiceId, label = "Low", score = 0, orderNo = 1 } },
                },
            },
            scoringRules = new[]
            {
                // "RuntimeLevel" (the level the earlier answer scored) is gone.
                new { min = 0, max = 5, level = "SomeOtherLevel", interpretation = "x", advice = "y" },
            },
        };

        var response = await _client.PutAsJsonAsync($"/api/admin/instruments/{instrumentId}", updateRequest);

        response.IsSuccessStatusCode.Should().BeFalse("a Result already references the 'RuntimeLevel' scoring level");
        ((int)response.StatusCode).Should().Be(400);
    }

    private async Task ActivateAndAnswerAsync(int instrumentId, int questionId, int choiceId)
    {
        (await _client.PatchAsync($"/api/admin/instruments/{instrumentId}/activate", null)).EnsureSuccessStatusCode();

        var username = $"user-{Guid.NewGuid():N}";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { username, email = $"{username}@example.com", password = "correcthorsebattery" });
        registerResponse.EnsureSuccessStatusCode();
        var token = (await registerResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        using var userClient = _factory.CreateClient();
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var startResponse = await userClient.PostAsync("/api/sessions", null);
        startResponse.EnsureSuccessStatusCode();
        var sessionId = (await startResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("sessionId").GetGuid();

        (await userClient.PostAsJsonAsync($"/api/sessions/{sessionId}/answers", new { questionId, choiceId }))
            .EnsureSuccessStatusCode();
    }

    private async Task<JsonElement> CreateInstrumentAsync()
    {
        var code = $"UPD-{Guid.NewGuid():N}"[..12];
        var createRequest = new
        {
            code,
            name = "Update-instrument test",
            version = "1.0",
            source = "integration-test",
            isActive = true,
            questions = new[]
            {
                new
                {
                    text = "Runtime-created question",
                    orderNo = 1,
                    choices = new[]
                    {
                        new { label = "Low", score = 0, orderNo = 1 },
                        new { label = "High", score = 5, orderNo = 2 },
                    },
                },
            },
            scoringRules = new[]
            {
                new { min = 0, max = 5, level = "RuntimeLevel", interpretation = "Created at runtime", advice = "Runtime advice" },
            },
            riskRules = Array.Empty<object>(),
        };

        var response = await _client.PostAsJsonAsync("/api/admin/instruments", createRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        using var loginClient = _factory.CreateClient();
        var response = await loginClient.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail = MindCheckApiFactory.TestAdminUsername, password = MindCheckApiFactory.TestAdminPassword });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }
}
