using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;

namespace MindCheck.Application.UseCases;

public sealed class ResetPasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public ResetPasswordUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher, TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var token = request.Token?.Trim() ?? string.Empty;
        var user = string.IsNullOrEmpty(token)
            ? null
            : await _userRepository.GetByPasswordResetTokenAsync(token, cancellationToken);

        if (user is null
            || user.PasswordResetTokenExpiresAt is null
            || user.PasswordResetTokenExpiresAt < _timeProvider.GetUtcNow())
        {
            throw new InvalidOrExpiredResetTokenException();
        }

        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new InvalidAuthRequestException("Password must be at least 8 characters long.");
        }

        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        user.ClearPasswordResetToken();
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
