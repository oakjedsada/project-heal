using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public sealed class RiskEvaluator : IRiskEvaluator
{
    public RiskEvaluationResult Evaluate(
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<RiskRule> riskRules)
    {
        var scoreByQuestion = answers.ToDictionary(a => a.QuestionId, a => a.Score);

        var triggeredActions = riskRules
            .Where(rule => scoreByQuestion.TryGetValue(rule.QuestionId, out var score)
                           && rule.Condition.IsSatisfiedBy(score))
            .Select(rule => rule.Action)
            .ToList();

        return new RiskEvaluationResult(triggeredActions.Count > 0, triggeredActions);
    }
}
