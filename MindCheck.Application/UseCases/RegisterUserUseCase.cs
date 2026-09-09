using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

public sealed class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator tokenGenerator,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<AuthTokenResponse> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (username.Length < 3)
        {
            throw new InvalidAuthRequestException("Username must be at least 3 characters long.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            throw new InvalidAuthRequestException("Password must be at least 8 characters long.");
        }

        var existing = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existing is not null)
        {
            throw new DuplicateUsernameException(username);
        }

        // Self-registration can only ever create a User account — minting an
        // Admin requires an existing admin, via AdminUsersController.
        var user = new User(
            new UserId(Guid.NewGuid()),
            username,
            _passwordHasher.Hash(request.Password),
            UserRole.User,
            _timeProvider.GetUtcNow());

        await _userRepository.AddAsync(user, cancellationToken);

        var token = _tokenGenerator.GenerateToken(user.Id, user.Username, user.Role);
        return new AuthTokenResponse(token, user.Id.Value, user.Username, user.Role.ToString());
    }
}
