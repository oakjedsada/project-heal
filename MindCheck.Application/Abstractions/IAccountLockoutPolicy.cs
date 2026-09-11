namespace MindCheck.Application.Abstractions;

public interface IAccountLockoutPolicy
{
    int MaxFailedAttempts { get; }

    TimeSpan LockoutDuration { get; }
}
