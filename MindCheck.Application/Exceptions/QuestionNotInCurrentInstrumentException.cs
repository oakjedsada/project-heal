using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class QuestionNotInCurrentInstrumentException : Exception
{
    public QuestionId QuestionId { get; }
    public InstrumentId CurrentInstrumentId { get; }

    public QuestionNotInCurrentInstrumentException(QuestionId questionId, InstrumentId currentInstrumentId)
        : base($"Question {questionId.Value} does not belong to the session's current instrument {currentInstrumentId.Value}.")
    {
        QuestionId = questionId;
        CurrentInstrumentId = currentInstrumentId;
    }
}
