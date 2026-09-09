using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Session
{
    public SessionId Id { get; }
    public UserId UserId { get; }
    public string AnonToken { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? ConsentAt { get; }
    public SessionState State { get; private set; }

    public Session(SessionId id, UserId userId, string anonToken, DateTimeOffset startedAt, DateTimeOffset? consentAt, SessionState state)
    {
        Id = id;
        UserId = userId;
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
