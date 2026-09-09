using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class SessionAccessDeniedException : Exception
{
    public SessionId SessionId { get; }

    public SessionAccessDeniedException(SessionId sessionId)
        : base($"Session {sessionId.Value} does not belong to the current user.")
    {
        SessionId = sessionId;
    }
}
