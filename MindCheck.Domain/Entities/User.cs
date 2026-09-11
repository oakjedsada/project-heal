using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class User
{
    public UserId Id { get; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTimeOffset? PasswordResetTokenExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    // Bumped whenever a password changes (self-service reset or admin edit)
    // or the user explicitly asks to sign out everywhere. Embedded as a JWT
    // claim; a token whose value no longer matches the current one is
    // rejected at validation time — the only way this codebase can revoke an
    // already-issued token without a separate blacklist store.
    public int TokenVersion { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedOutUntil { get; private set; }

    public User(UserId id, string username, string email, string passwordHash, UserRole role, DateTimeOffset createdAt)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
        TokenVersion = 0;
        FailedLoginAttempts = 0;
    }

    public void ChangeUsername(string newUsername)
    {
        Username = newUsername;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        TokenVersion++;
    }

    public void IncrementTokenVersion()
    {
        TokenVersion++;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockedOutUntil is { } until && until > now;

    public void RegisterFailedLogin(DateTimeOffset now, int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedOutUntil = now.Add(lockoutDuration);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void SetPasswordResetToken(string token, DateTimeOffset expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }
}
