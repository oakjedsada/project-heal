using System.Security.Cryptography;
using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;

namespace MindCheck.Application.UseCases;

public sealed class ForgotPasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordResetLinkBuilder _linkBuilder;
    private readonly TimeProvider _timeProvider;

    public ForgotPasswordUseCase(
        IUserRepository userRepository,
        IEmailSender emailSender,
        IPasswordResetLinkBuilder linkBuilder,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _linkBuilder = linkBuilder;
        _timeProvider = timeProvider;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            // Same response shape as the "email sent successfully" path below,
            // so this endpoint can't be used to enumerate registered addresses.
            return new ForgotPasswordResponse(null);
        }

        var token = RandomNumberGenerator.GetHexString(48);
        var expiresAt = _timeProvider.GetUtcNow() + _linkBuilder.TokenLifetime;
        user.SetPasswordResetToken(token, expiresAt);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var resetLink = _linkBuilder.BuildResetLink(token);
        var emailSent = await _emailSender.TrySendPasswordResetEmailAsync(user.Email, resetLink, cancellationToken);

        // Only surface the link directly when we couldn't actually email it —
        // real delivery means the caller never sees the token itself.
        return new ForgotPasswordResponse(emailSent ? null : resetLink);
    }
}
