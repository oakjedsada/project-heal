namespace MindCheck.Application.Exceptions;

// Thrown instead of InvalidCredentialsException once the lockout threshold
// is hit — unlike a wrong password, this doesn't need to hide account
// existence: the attempt count itself already narrowed that down.
public sealed class AccountLockedOutException : Exception
{
    public DateTimeOffset LockedOutUntil { get; }

    public AccountLockedOutException(DateTimeOffset lockedOutUntil)
        : base($"Account temporarily locked due to too many failed login attempts. Try again after {lockedOutUntil:O}.")
    {
        LockedOutUntil = lockedOutUntil;
    }
}
