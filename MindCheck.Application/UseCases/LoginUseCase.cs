using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;

namespace MindCheck.Application.UseCases;

public sealed class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IAccountLockoutPolicy _lockoutPolicy;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator tokenGenerator,
        TimeProvider timeProvider,
        IAccountLockoutPolicy lockoutPolicy)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
        _lockoutPolicy = lockoutPolicy;
    }

    public async Task<AuthTokenResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail ?? string.Empty, cancellationToken)
            ?? throw new InvalidCredentialsException();

        var now = _timeProvider.GetUtcNow();
        if (user.IsLockedOut(now))
        {
            throw new AccountLockedOutException(user.LockedOutUntil!.Value);
        }

        if (!_passwordHasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
        {
            user.RegisterFailedLogin(now, _lockoutPolicy.MaxFailedAttempts, _lockoutPolicy.LockoutDuration);
            await _userRepository.UpdateAsync(user, cancellationToken);
            throw new InvalidCredentialsException();
        }

        user.RegisterSuccessfulLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = _tokenGenerator.GenerateToken(user.Id, user.Username, user.Role, user.TokenVersion);
        return new AuthTokenResponse(token, user.Id.Value, user.Username, user.Role.ToString());
    }
}
