namespace MindCheck.Application.Dtos.Admin;

public sealed record UserDto(Guid UserId, string Username, string Email, string Role, DateTimeOffset CreatedAt);

public sealed record CreateUserRequest(string Username, string Email, string Password, string Role);

public sealed record ChangeUserRoleRequest(string Role);
