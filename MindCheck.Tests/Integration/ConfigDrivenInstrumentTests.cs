using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MindCheck.Tests.Integration;

// This is the proof that MindCheck is genuinely config-driven: create a whole
// new assessment instrument and wire it into the flow, entirely through the
// admin HTTP API (the same API the admin UI calls) — then, in the SAME
// running process with no restart and no redeploy, start a public session and
// walk straight into that brand-new instrument. If this test needs a code
// change or a process restart to pass, the "config-driven" claim is false.
public sealed class ConfigDrivenInstrumentTests : IClassFixture<MindCheckApiFactory>
{
    private readonly HttpClient _client;

    public ConfigDrivenInstrumentTests(MindCheckApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task NewInstrumentCreatedViaAdminApi_IsReachableInASessionImmediately_NoRestartRequired()
    {
        var token = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create a brand-new instrument end-to-end through the admin API —
        // question, choices, and scoring rule all in one request.
        var createRequest = new
        {
            code = "ZTEST",
            name = "Z Test Instrument (created at runtime)",
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

        var createResponse = await _client.PostAsJsonAsync("/api/admin/instruments", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        var newInstrumentId = created.GetProperty("instrumentId").GetInt32();
        var newQuestion = created.GetProperty("questions")[0];
        var newQuestionId = newQuestion.GetProperty("questionId").GetInt32();
        var lowChoiceId = newQuestion.GetProperty("choices")[0].GetProperty("choiceId").GetInt32();

        // 2. Wire it in: 2Q's "Negative" outcome currently just ends the
        // session (no transition exists for it) — point it at the new
        // instrument instead, purely via a data row.
        var instrumentsResponse = await _client.GetAsync("/api/admin/instruments");
        instrumentsResponse.EnsureSuccessStatusCode();
        var instruments = await instrumentsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var twoQId = instruments.EnumerateArray()
            .First(i => i.GetProperty("code").GetString() == "2Q")
            .GetProperty("instrumentId").GetInt32();

        var transitionRequest = new
        {
            fromInstrumentId = twoQId,
            conditionType = "ScoreLevelEquals",
            questionId = (int?)null,
            conditionValue = "Negative",
            toInstrumentId = newInstrumentId,
        };
        var transitionResponse = await _client.PostAsJsonAsync("/api/admin/flow-transitions", transitionRequest);
        transitionResponse.EnsureSuccessStatusCode();

        // 3. Now act as an ordinary end user (a fresh registered account, not
        // the admin), in the same process, same run, no restart in between.
        var userToken = await RegisterUserAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var sessionResponse = await _client.PostAsync("/api/sessions", content: null);
        sessionResponse.EnsureSuccessStatusCode();
        var session = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>();
        var sessionId = session.GetProperty("sessionId").GetGuid();

        for (var questionId = 1; questionId <= 5; questionId++)
        {
            await AnswerAsync(sessionId, questionId, questionId * 10 + 1); // ST-5 mildest
        }

        await AnswerAsync(sessionId, 6, 61); // 2Q both negative -> level "Negative"
        await AnswerAsync(sessionId, 7, 71);

        // 4. The very next question must be the one we just created.
        var nextResponse = await _client.GetAsync($"/api/sessions/{sessionId}/next");
        nextResponse.EnsureSuccessStatusCode();
        var next = await nextResponse.Content.ReadFromJsonAsync<JsonElement>();

        next.GetProperty("isComplete").GetBoolean().Should().BeFalse();
        next.GetProperty("question").GetProperty("questionId").GetInt32().Should().Be(newQuestionId);

        // 5. Answer it, then confirm the final result reflects exactly what
        // was configured moments ago — not a code constant.
        await AnswerAsync(sessionId, newQuestionId, lowChoiceId);

        var resultResponse = await _client.GetAsync($"/api/sessions/{sessionId}/result");
        resultResponse.EnsureSuccessStatusCode();
        var result = await resultResponse.Content.ReadFromJsonAsync<JsonElement>();

        result.GetProperty("isComplete").GetBoolean().Should().BeTrue();
        result.GetProperty("nextAction").GetString().Should().Be("completed");

        var lastResult = result.GetProperty("results").EnumerateArray().Last();
        lastResult.GetProperty("instrumentCode").GetString().Should().Be("ZTEST");
        lastResult.GetProperty("level").GetString().Should().Be("RuntimeLevel");
        lastResult.GetProperty("interpretation").GetString().Should().Be("Created at runtime");
        lastResult.GetProperty("advice").GetString().Should().Be("Runtime advice");
    }

    [Fact]
    public async Task CreateInstrument_WithoutAuthToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync("/api/admin/instruments", new { code = "NOPE" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { username = MindCheckApiFactory.TestAdminUsername, password = "definitely-wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { username = MindCheckApiFactory.TestAdminUsername, password = MindCheckApiFactory.TestAdminPassword });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private async Task<string> RegisterUserAsync()
    {
        var username = $"user-{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { username, password = "correcthorsebattery" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private async Task AnswerAsync(Guid sessionId, int questionId, int choiceId)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/answers",
            new { questionId, choiceId });
        response.EnsureSuccessStatusCode();
    }
}
