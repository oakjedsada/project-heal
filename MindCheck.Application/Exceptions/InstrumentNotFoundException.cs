using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class InstrumentNotFoundException : Exception
{
    public InstrumentId InstrumentId { get; }

    public InstrumentNotFoundException(InstrumentId instrumentId)
        : base($"Instrument {instrumentId.Value} was not found.")
    {
        InstrumentId = instrumentId;
    }
}
