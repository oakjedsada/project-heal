using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Exceptions;

public sealed class UserNotFoundException : Exception
{
    public UserId UserId { get; }

    public UserNotFoundException(UserId userId)
        : base($"User {userId.Value} was not found.")
    {
        UserId = userId;
    }
}
