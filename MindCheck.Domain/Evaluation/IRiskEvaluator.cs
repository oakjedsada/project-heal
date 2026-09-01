using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public interface IRiskEvaluator
{
    RiskEvaluationResult Evaluate(
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<RiskRule> riskRules);
}
