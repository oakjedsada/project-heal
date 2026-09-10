using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Application.Validation;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

// Backs the admin "edit user" page: username, email, role, and an optional
// password reset, all in one request. Username/email uniqueness checks
// exclude the user's own current row, so re-saving unchanged values doesn't
// spuriously collide with themselves.
public sealed class UpdateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> ExecuteAsync(UserId id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        var username = request.Username?.Trim() ?? string.Empty;
        if (username.Length < 3)
        {
            throw new InvalidAdminRequestException("Username must be at least 3 characters long.");
        }

        var email = request.Email?.Trim() ?? string.Empty;
        if (!EmailValidator.IsValid(email))
        {
            throw new InvalidAdminRequestException("A valid email address is required.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var newRole))
        {
            throw new InvalidAdminRequestException($"Unknown role '{request.Role}'.");
        }

        var existingByUsername = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existingByUsername is not null && existingByUsername.Id != id)
        {
            throw new DuplicateUsernameException(username);
        }

        var existingByEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            throw new DuplicateEmailException(email);
        }

        if (user.Role == UserRole.Admin && newRole == UserRole.User
            && await _userRepository.CountByRoleAsync(UserRole.Admin, cancellationToken) <= 1)
        {
            throw new InvalidAdminRequestException("Cannot demote the last remaining admin.");
        }

        user.ChangeUsername(username);
        user.ChangeEmail(email);
        user.ChangeRole(newRole);

        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password.Length < 8)
            {
                throw new InvalidAdminRequestException("Password must be at least 8 characters long.");
            }

            user.ChangePassword(_passwordHasher.Hash(request.Password));
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserDto(user.Id.Value, user.Username, user.Email, user.Role.ToString(), user.CreatedAt);
    }
}
