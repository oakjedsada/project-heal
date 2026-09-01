using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class SessionAlreadyFinishedException : Exception
{
    public SessionId SessionId { get; }

    public SessionAlreadyFinishedException(SessionId sessionId)
        : base($"Session {sessionId.Value} has already finished and cannot accept more answers.")
    {
        SessionId = sessionId;
    }
}
