using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Session
{
    public SessionId Id { get; }
    public string AnonToken { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? ConsentAt { get; }
    public SessionState State { get; private set; }

    public Session(SessionId id, string anonToken, DateTimeOffset startedAt, DateTimeOffset? consentAt, SessionState state)
    {
        Id = id;
        AnonToken = anonToken;
        StartedAt = startedAt;
        ConsentAt = consentAt;
        State = state;
    }

    public void AdvanceTo(SessionState newState)
    {
        State = newState;
    }
}
