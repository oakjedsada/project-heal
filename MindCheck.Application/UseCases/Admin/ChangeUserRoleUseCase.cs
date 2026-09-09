using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class ChangeUserRoleUseCase
{
    private readonly IUserRepository _userRepository;

    public ChangeUserRoleUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> ExecuteAsync(UserId id, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var newRole))
        {
            throw new InvalidAdminRequestException($"Unknown role '{request.Role}'.");
        }

        if (user.Role == UserRole.Admin && newRole == UserRole.User
            && await _userRepository.CountByRoleAsync(UserRole.Admin, cancellationToken) <= 1)
        {
            throw new InvalidAdminRequestException("Cannot demote the last remaining admin.");
        }

        user.ChangeRole(newRole);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserDto(user.Id.Value, user.Username, user.Role.ToString(), user.CreatedAt);
    }
}
