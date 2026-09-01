using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Exceptions;

public sealed class IncompleteAssessmentException : Exception
{
    public IReadOnlyList<QuestionId> MissingQuestionIds { get; }

    public IncompleteAssessmentException(IReadOnlyList<QuestionId> missingQuestionIds)
        : base($"Assessment is missing answers for {missingQuestionIds.Count} question(s).")
    {
        MissingQuestionIds = missingQuestionIds;
    }
}
