using MindCheck.Application.Abstractions;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

// Bumps TokenVersion so every JWT issued before this call is rejected at
// validation time (see Program.cs's OnTokenValidated handler) — the
// self-service "sign out everywhere" action.
public sealed class LogoutAllSessionsUseCase
{
    private readonly IUserRepository _userRepository;

    public LogoutAllSessionsUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(UserId callerUserId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(callerUserId, cancellationToken)
            ?? throw new UserNotFoundException(callerUserId);

        user.IncrementTokenVersion();
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
