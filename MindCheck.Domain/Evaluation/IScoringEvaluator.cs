using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public interface IScoringEvaluator
{
    ScoringResult Evaluate(
        IReadOnlyCollection<QuestionId> expectedQuestionIds,
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<ScoringRule> scoringRules);
}
