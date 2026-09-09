namespace MindCheck.Application.Dtos.Admin;

public sealed record UserDto(Guid UserId, string Username, string Role, DateTimeOffset CreatedAt);

public sealed record CreateUserRequest(string Username, string Password, string Role);

public sealed record ChangeUserRoleRequest(string Role);
