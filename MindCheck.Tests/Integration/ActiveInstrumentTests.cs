using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MindCheck.Tests.Integration;

// Covers SetActiveInstrumentUseCase (ADR 0016): an admin picking which
// instrument every new session starts with, instead of manually managing
// flow_transitions rows themselves.
public sealed class ActiveInstrumentTests : IClassFixture<MindCheckApiFactory>
{
    private readonly HttpClient _client;

    public ActiveInstrumentTests(MindCheckApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Activate_MakesInstrumentTheOnlyActiveStart_AndNewSessionsStartThere()
    {
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var (instrumentId, questionId) = await CreateInstrumentAsync();

        var activateResponse = await _client.PatchAsync($"/api/admin/instruments/{instrumentId}/activate", null);
        activateResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.GetAsync("/api/admin/instruments");
        listResponse.EnsureSuccessStatusCode();
        var instruments = (await listResponse.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();

        instruments
            .Count(i => i.GetProperty("isActiveStart").GetBoolean())
            .Should().Be(1, "activating one instrument must deactivate every other start transition");
        instruments
            .Single(i => i.GetProperty("isActiveStart").GetBoolean())
            .GetProperty("instrumentId").GetInt32()
            .Should().Be(instrumentId);

        // A brand-new session, started right after, must land on this
        // instrument's question — no restart needed to see the change.
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await RegisterUserAsync());

        var startResponse = await _client.PostAsync("/api/sessions", null);
        startResponse.EnsureSuccessStatusCode();
        var sessionId = (await startResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("sessionId").GetGuid();

        var nextResponse = await _client.GetAsync($"/api/sessions/{sessionId}/next");
        nextResponse.EnsureSuccessStatusCode();
        var next = await nextResponse.Content.ReadFromJsonAsync<JsonElement>();
        next.GetProperty("question").GetProperty("questionId").GetInt32().Should().Be(questionId);
    }

    [Fact]
    public async Task Activate_UnknownInstrument_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsAdminAsync());

        var response = await _client.PatchAsync("/api/admin/instruments/999999/activate", null);

        ((int)response.StatusCode).Should().Be(404);
    }

    private async Task<(int InstrumentId, int QuestionId)> CreateInstrumentAsync()
    {
        var code = $"ACT-{Guid.NewGuid():N}"[..12];
        var createRequest = new
        {
            code,
            name = "Active-instrument test",
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
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();

        var instrumentId = created.GetProperty("instrumentId").GetInt32();
        var questionId = created.GetProperty("questions")[0].GetProperty("questionId").GetInt32();
        return (instrumentId, questionId);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail = MindCheckApiFactory.TestAdminUsername, password = MindCheckApiFactory.TestAdminPassword });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private async Task<string> RegisterUserAsync()
    {
        var username = $"user-{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { username, email = $"{username}@example.com", password = "correcthorsebattery" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }
}
