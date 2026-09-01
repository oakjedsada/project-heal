using FluentAssertions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.Evaluation;
using MindCheck.Domain.ValueObjects;
using Xunit;

namespace MindCheck.Tests.Evaluation;

public sealed class AssessmentFlowEngineTests
{
    private static readonly InstrumentId FromInstrumentId = new(1);
    private static readonly InstrumentId NextInstrumentId = new(2);
    private static readonly QuestionId GateQuestionId = new(9);
    private static readonly ChoiceId AnyChoiceId = new(99);

    private static readonly ScoringResult PositiveScoring = new(2, "Positive", "interp", "advice");
    private static readonly ScoringResult NegativeScoring = new(0, "Negative", "interp", "advice");
    private static readonly RiskEvaluationResult NotEscalated = new(false, Array.Empty<string>());
    private static readonly RiskEvaluationResult Escalated = new(true, new[] { "emergency" });

    private readonly IAssessmentFlowEngine _sut = new AssessmentFlowEngine();

    [Fact]
    public void Decide_RiskEscalated_ReturnsEmergencyRegardlessOfTransitions()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.Always, null, null, NextInstrumentId),
        };

        var result = _sut.Decide(Escalated, NegativeScoring, Array.Empty<Answer>(), transitions);

        result.Outcome.Should().Be(FlowOutcome.Emergency);
        result.NextInstrumentId.Should().BeNull();
    }

    [Fact]
    public void Decide_AlwaysConditionTransition_ContinuesToConfiguredInstrument()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.Always, null, null, NextInstrumentId),
        };

        var result = _sut.Decide(NotEscalated, NegativeScoring, Array.Empty<Answer>(), transitions);

        result.Outcome.Should().Be(FlowOutcome.ContinueToNextInstrument);
        result.NextInstrumentId.Should().Be(NextInstrumentId);
    }

    [Fact]
    public void Decide_ScoreLevelMatchesTransitionCondition_ContinuesToConfiguredInstrument()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.ScoreLevelEquals, null, "Positive", NextInstrumentId),
        };

        var result = _sut.Decide(NotEscalated, PositiveScoring, Array.Empty<Answer>(), transitions);

        result.Outcome.Should().Be(FlowOutcome.ContinueToNextInstrument);
        result.NextInstrumentId.Should().Be(NextInstrumentId);
    }

    [Fact]
    public void Decide_ScoreLevelDoesNotMatchTransitionCondition_ReturnsCompleted()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.ScoreLevelEquals, null, "Positive", NextInstrumentId),
        };

        var result = _sut.Decide(NotEscalated, NegativeScoring, Array.Empty<Answer>(), transitions);

        result.Outcome.Should().Be(FlowOutcome.Completed);
    }

    [Fact]
    public void Decide_SpecificQuestionScoreAtOrAboveThreshold_ContinuesToConfiguredInstrument()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.QuestionScoreAtLeast, GateQuestionId, "1", NextInstrumentId),
        };
        var answers = new[] { new Answer(GateQuestionId, AnyChoiceId, 1) };

        var result = _sut.Decide(NotEscalated, NegativeScoring, answers, transitions);

        result.Outcome.Should().Be(FlowOutcome.ContinueToNextInstrument);
        result.NextInstrumentId.Should().Be(NextInstrumentId);
    }

    [Fact]
    public void Decide_SpecificQuestionScoreBelowThreshold_ReturnsCompleted()
    {
        var transitions = new[]
        {
            new FlowTransition(1, FromInstrumentId, FlowConditionType.QuestionScoreAtLeast, GateQuestionId, "1", NextInstrumentId),
        };
        var answers = new[] { new Answer(GateQuestionId, AnyChoiceId, 0) };

        var result = _sut.Decide(NotEscalated, NegativeScoring, answers, transitions);

        result.Outcome.Should().Be(FlowOutcome.Completed);
    }

    [Fact]
    public void Decide_NoTransitionsProvided_ReturnsCompleted()
    {
        var result = _sut.Decide(NotEscalated, NegativeScoring, Array.Empty<Answer>(), Array.Empty<FlowTransition>());

        result.Outcome.Should().Be(FlowOutcome.Completed);
    }
}
