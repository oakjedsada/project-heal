using MindCheck.Domain.Entities;
using MindCheck.Domain.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public sealed class ScoringEvaluator : IScoringEvaluator
{
    public ScoringResult Evaluate(
        IReadOnlyCollection<QuestionId> expectedQuestionIds,
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<ScoringRule> scoringRules)
    {
        var answeredQuestionIds = answers.Select(a => a.QuestionId).ToHashSet();
        var missingQuestionIds = expectedQuestionIds
            .Where(questionId => !answeredQuestionIds.Contains(questionId))
            .ToList();

        if (missingQuestionIds.Count > 0)
        {
            throw new IncompleteAssessmentException(missingQuestionIds);
        }

        var totalScore = answers.Sum(a => a.Score);

        var matchingRule = scoringRules.FirstOrDefault(rule => rule.Range.Contains(totalScore));
        if (matchingRule is null)
        {
            throw new ScoringRuleNotFoundException(totalScore);
        }

        return new ScoringResult(totalScore, matchingRule.Level, matchingRule.Interpretation, matchingRule.Advice);
    }
}
