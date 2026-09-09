using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class User
{
    public UserId Id { get; }
    public string Username { get; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public User(UserId id, string username, string passwordHash, UserRole role, DateTimeOffset createdAt)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }
}
