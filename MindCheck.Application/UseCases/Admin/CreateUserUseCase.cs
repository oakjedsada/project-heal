using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

// The one place an Admin account can be minted after the bootstrap admin —
// self-registration (RegisterUserUseCase) can only ever create a User.
public sealed class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public CreateUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher, TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<UserDto> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (username.Length < 3)
        {
            throw new InvalidAdminRequestException("Username must be at least 3 characters long.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            throw new InvalidAdminRequestException("Password must be at least 8 characters long.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            throw new InvalidAdminRequestException($"Unknown role '{request.Role}'.");
        }

        var existing = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existing is not null)
        {
            throw new DuplicateUsernameException(username);
        }

        var user = new User(
            new UserId(Guid.NewGuid()),
            username,
            _passwordHasher.Hash(request.Password),
            role,
            _timeProvider.GetUtcNow());

        await _userRepository.AddAsync(user, cancellationToken);

        return new UserDto(user.Id.Value, user.Username, user.Role.ToString(), user.CreatedAt);
    }
}
