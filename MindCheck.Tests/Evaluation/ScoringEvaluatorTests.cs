using FluentAssertions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.Evaluation;
using MindCheck.Domain.Exceptions;
using MindCheck.Domain.ValueObjects;
using Xunit;

namespace MindCheck.Tests.Evaluation;

public sealed class ScoringEvaluatorTests
{
    private static readonly InstrumentId InstrumentId = new(1);
    private static readonly QuestionId QuestionOne = new(1);
    private static readonly QuestionId QuestionTwo = new(2);
    private static readonly ChoiceId AnyChoiceId = new(99);

    // Fixture deliberately leaves 0-1 and 11+ uncovered so "outside every range" is reachable.
    private static readonly IReadOnlyCollection<ScoringRule> Rules = new List<ScoringRule>
    {
        new(1, InstrumentId, new ScoreRange(2, 4), "Low", "Low interpretation", "Low advice"),
        new(2, InstrumentId, new ScoreRange(5, 7), "Medium", "Medium interpretation", "Medium advice"),
        new(3, InstrumentId, new ScoreRange(8, 10), "High", "High interpretation", "High advice"),
    };

    private readonly IScoringEvaluator _sut = new ScoringEvaluator();

    [Theory]
    [InlineData(2, "Low")]
    [InlineData(4, "Low")]
    [InlineData(5, "Medium")]
    [InlineData(7, "Medium")]
    [InlineData(8, "High")]
    [InlineData(10, "High")]
    public void Evaluate_ScoreAtRangeBoundary_ReturnsMatchingLevel(int totalScore, string expectedLevel)
    {
        var answers = new List<Answer> { new(QuestionOne, AnyChoiceId, totalScore) };

        var result = _sut.Evaluate(new[] { QuestionOne }, answers, Rules);

        result.Level.Should().Be(expectedLevel);
        result.TotalScore.Should().Be(totalScore);
    }

    [Fact]
    public void Evaluate_ScoreBelowAllDefinedRanges_ThrowsScoringRuleNotFoundException()
    {
        var answers = new List<Answer> { new(QuestionOne, AnyChoiceId, 1) };

        var act = () => _sut.Evaluate(new[] { QuestionOne }, answers, Rules);

        act.Should().Throw<ScoringRuleNotFoundException>()
            .Which.TotalScore.Should().Be(1);
    }

    [Fact]
    public void Evaluate_ScoreAboveAllDefinedRanges_ThrowsScoringRuleNotFoundException()
    {
        var answers = new List<Answer> { new(QuestionOne, AnyChoiceId, 11) };

        var act = () => _sut.Evaluate(new[] { QuestionOne }, answers, Rules);

        act.Should().Throw<ScoringRuleNotFoundException>()
            .Which.TotalScore.Should().Be(11);
    }

    [Fact]
    public void Evaluate_AnswersMissingForExpectedQuestion_ThrowsIncompleteAssessmentException()
    {
        var expectedQuestionIds = new[] { QuestionOne, QuestionTwo };
        var answers = new List<Answer> { new(QuestionOne, AnyChoiceId, 2) };

        var act = () => _sut.Evaluate(expectedQuestionIds, answers, Rules);

        act.Should().Throw<IncompleteAssessmentException>()
            .Which.MissingQuestionIds.Should().ContainSingle().Which.Should().Be(QuestionTwo);
    }
}
