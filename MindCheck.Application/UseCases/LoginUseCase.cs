using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;

namespace MindCheck.Application.UseCases;

public sealed class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _tokenGenerator;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthTokenResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username ?? string.Empty, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!_passwordHasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = _tokenGenerator.GenerateToken(user.Id, user.Username, user.Role);
        return new AuthTokenResponse(token, user.Id.Value, user.Username, user.Role.ToString());
    }
}
