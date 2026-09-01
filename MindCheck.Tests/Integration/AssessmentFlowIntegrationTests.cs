using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MindCheck.Tests.Integration;

// Walks the real HTTP API through the three reachable paths of the seeded
// state machine: ST-5 -> 2Q (negative) -> done; ST-5 -> 2Q (positive) -> 9Q
// (item 9 negative) -> done; and ST-5 -> 2Q (positive) -> 9Q (item 9 positive)
// -> 8Q (risk item positive) -> emergency.
public sealed class AssessmentFlowIntegrationTests : IClassFixture<MindCheckApiFactory>
{
    private static readonly int[] St5QuestionIds = { 1, 2, 3, 4, 5 };
    private static readonly int[] NineQQuestionIds = Enumerable.Range(8, 9).ToArray();
    private static readonly int[] EightQQuestionIds = Enumerable.Range(17, 8).ToArray();
    private const int TwoQPositiveQuestionId = 6;
    private const int TwoQNegativeQuestionId = 7;
    private const int NineQItem9QuestionId = 16;
    private const int EightQLastQuestionId = 24;

    private readonly HttpClient _client;

    public AssessmentFlowIntegrationTests(MindCheckApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Flow_2QNegative_CompletesAfter2QWithTwoResults()
    {
        var sessionId = await StartSessionAsync();
        await AnswerAllAsync(sessionId, St5QuestionIds, score: 0);
        await AnswerAsync(sessionId, TwoQPositiveQuestionId, score: 0);
        await AnswerAsync(sessionId, TwoQNegativeQuestionId, score: 0);

        var next = await GetNextAsync(sessionId);
        next.GetProperty("isComplete").GetBoolean().Should().BeTrue();

        var result = await GetResultAsync(sessionId);
        result.GetProperty("isComplete").GetBoolean().Should().BeTrue();
        result.GetProperty("nextAction").GetString().Should().Be("completed");
        result.GetProperty("results").GetArrayLength().Should().Be(2);
        result.GetProperty("helpResources").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Flow_2QPositiveItem9Negative_ReachesAndCompletesAt9QWithThreeResults()
    {
        var sessionId = await StartSessionAsync();
        await AnswerAllAsync(sessionId, St5QuestionIds, score: 0);
        await AnswerAsync(sessionId, TwoQPositiveQuestionId, score: 1);
        await AnswerAsync(sessionId, TwoQNegativeQuestionId, score: 0);
        await AnswerAllAsync(sessionId, NineQQuestionIds, score: 0);

        var result = await GetResultAsync(sessionId);
        result.GetProperty("isComplete").GetBoolean().Should().BeTrue();
        result.GetProperty("nextAction").GetString().Should().Be("completed");

        var results = result.GetProperty("results").EnumerateArray().ToList();
        results.Should().HaveCount(3);
        results[^1].GetProperty("instrumentCode").GetString().Should().Be("9Q");
    }

    [Fact]
    public async Task Flow_Item9PositiveAnd8QRiskItemPositive_ReachesEightQAndEscalatesToEmergency()
    {
        var sessionId = await StartSessionAsync();
        await AnswerAllAsync(sessionId, St5QuestionIds, score: 0);
        await AnswerAsync(sessionId, TwoQPositiveQuestionId, score: 1);
        await AnswerAsync(sessionId, TwoQNegativeQuestionId, score: 0);
        await AnswerAllAsync(sessionId, NineQQuestionIds.Where(id => id != NineQItem9QuestionId), score: 0);
        await AnswerAsync(sessionId, NineQItem9QuestionId, score: 1);

        var nextAfter9Q = await GetNextAsync(sessionId);
        nextAfter9Q.GetProperty("isComplete").GetBoolean().Should().BeFalse();
        nextAfter9Q.GetProperty("question").GetProperty("questionId").GetInt32().Should().Be(EightQQuestionIds[0]);

        await AnswerAllAsync(sessionId, EightQQuestionIds.Where(id => id != EightQLastQuestionId), score: 0);
        await AnswerAsync(sessionId, EightQLastQuestionId, score: 1);

        var result = await GetResultAsync(sessionId);
        result.GetProperty("isComplete").GetBoolean().Should().BeTrue();
        result.GetProperty("nextAction").GetString().Should().Be("emergency");

        var results = result.GetProperty("results").EnumerateArray().ToList();
        results.Should().HaveCount(4);
        results[^1].GetProperty("instrumentCode").GetString().Should().Be("8Q");

        // The safety path must never present a bare score: help resources are
        // mandatory whenever next_action is "emergency".
        result.GetProperty("helpResources").ValueKind.Should().Be(JsonValueKind.Array);
        result.GetProperty("helpResources").GetArrayLength().Should().BeGreaterThan(0);
    }

    private async Task<Guid> StartSessionAsync()
    {
        var response = await _client.PostAsync("/api/sessions", content: null);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("sessionId").GetGuid();
    }

    private async Task AnswerAllAsync(Guid sessionId, IEnumerable<int> questionIds, int score)
    {
        foreach (var questionId in questionIds)
        {
            await AnswerAsync(sessionId, questionId, score);
        }
    }

    private async Task AnswerAsync(Guid sessionId, int questionId, int score)
    {
        var choiceId = questionId * 10 + score + 1;
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/answers",
            new { questionId, choiceId });
        response.EnsureSuccessStatusCode();
    }

    private async Task<JsonElement> GetNextAsync(Guid sessionId)
    {
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/next");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<JsonElement> GetResultAsync(Guid sessionId)
    {
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/result");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
