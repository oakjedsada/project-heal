using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class SessionNotFoundException : Exception
{
    public SessionId SessionId { get; }

    public SessionNotFoundException(SessionId sessionId)
        : base($"Session {sessionId.Value} was not found.")
    {
        SessionId = sessionId;
    }
}
