using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class ChoiceNotFoundException : Exception
{
    public ChoiceId ChoiceId { get; }
    public QuestionId QuestionId { get; }

    public ChoiceNotFoundException(ChoiceId choiceId, QuestionId questionId)
        : base($"Choice {choiceId.Value} does not belong to question {questionId.Value}.")
    {
        ChoiceId = choiceId;
        QuestionId = questionId;
    }
}
