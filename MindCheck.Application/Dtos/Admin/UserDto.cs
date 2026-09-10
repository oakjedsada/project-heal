namespace MindCheck.Application.Dtos.Admin;

public sealed record UserDto(Guid UserId, string Username, string Email, string Role, DateTimeOffset CreatedAt);

public sealed record CreateUserRequest(string Username, string Email, string Password, string Role);

/// <summary>Password is optional — null or empty leaves the existing password unchanged.</summary>
public sealed record UpdateUserRequest(string Username, string Email, string Role, string? Password);
