using FluentAssertions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.Evaluation;
using MindCheck.Domain.ValueObjects;
using Xunit;

namespace MindCheck.Tests.Evaluation;

public sealed class RiskEvaluatorTests
{
    private static readonly InstrumentId InstrumentId = new(1);
    private static readonly QuestionId RiskQuestionId = new(9);
    private static readonly ChoiceId AnyChoiceId = new(99);
    private const string EmergencyAction = "emergency";

    private static readonly IReadOnlyCollection<RiskRule> Rules = new List<RiskRule>
    {
        new(1, InstrumentId, RiskQuestionId, new RiskCondition(RiskOperator.GreaterThanOrEqual, 1), EmergencyAction),
    };

    private readonly IRiskEvaluator _sut = new RiskEvaluator();

    [Fact]
    public void Evaluate_RiskRuleThresholdMet_ReturnsEscalated()
    {
        var answers = new List<Answer> { new(RiskQuestionId, AnyChoiceId, 1) };

        var result = _sut.Evaluate(answers, Rules);

        result.IsEscalated.Should().BeTrue();
        result.TriggeredActions.Should().ContainSingle().Which.Should().Be(EmergencyAction);
    }

    [Fact]
    public void Evaluate_RiskRuleThresholdNotMet_ReturnsNotEscalated()
    {
        var answers = new List<Answer> { new(RiskQuestionId, AnyChoiceId, 0) };

        var result = _sut.Evaluate(answers, Rules);

        result.IsEscalated.Should().BeFalse();
        result.TriggeredActions.Should().BeEmpty();
    }
}
