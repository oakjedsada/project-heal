using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public sealed class AssessmentFlowEngine : IAssessmentFlowEngine
{
    public FlowDecision Decide(
        RiskEvaluationResult riskResult,
        ScoringResult scoringResult,
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<FlowTransition> candidateTransitions)
    {
        if (riskResult.IsEscalated)
        {
            return FlowDecision.Emergency();
        }

        var scoreByQuestion = answers.ToDictionary(a => a.QuestionId, a => a.Score);

        var matchingTransition = candidateTransitions
            .Where(t => Matches(t, scoringResult, scoreByQuestion))
            .OrderBy(t => t.Id)
            .FirstOrDefault();

        return matchingTransition is null
            ? FlowDecision.Completed()
            : FlowDecision.ContinueTo(matchingTransition.ToInstrumentId);
    }

    private static bool Matches(
        FlowTransition transition,
        ScoringResult scoringResult,
        IReadOnlyDictionary<QuestionId, int> scoreByQuestion) =>
        transition.ConditionType switch
        {
            FlowConditionType.Always => true,
            FlowConditionType.ScoreLevelEquals =>
                string.Equals(transition.ConditionValue, scoringResult.Level, StringComparison.Ordinal),
            FlowConditionType.QuestionScoreAtLeast =>
                transition.QuestionId.HasValue
                && scoreByQuestion.TryGetValue(transition.QuestionId.Value, out var score)
                && int.TryParse(transition.ConditionValue, out var threshold)
                && score >= threshold,
            _ => false
        };
}
