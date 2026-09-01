namespace MindCheck.Domain.ValueObjects;

public enum SessionStatus
{
    InProgress,
    Completed,
    Emergency
}

public sealed class SessionState
{
    public SessionStatus Status { get; }
    public InstrumentId? CurrentInstrumentId { get; }

    private SessionState(SessionStatus status, InstrumentId? currentInstrumentId)
    {
        Status = status;
        CurrentInstrumentId = currentInstrumentId;
    }

    public static SessionState AtInstrument(InstrumentId instrumentId) => new(SessionStatus.InProgress, instrumentId);

    public static SessionState Completed() => new(SessionStatus.Completed, null);

    public static SessionState Emergency() => new(SessionStatus.Emergency, null);
}
