using Microsoft.Extensions.Options;
using MindCheck.Application.Abstractions;

namespace MindCheck.Api.Auth;

public sealed class AccountLockoutPolicy : IAccountLockoutPolicy
{
    private readonly AuthOptions _options;

    public AccountLockoutPolicy(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public int MaxFailedAttempts => _options.MaxFailedLoginAttempts;

    public TimeSpan LockoutDuration => TimeSpan.FromMinutes(_options.LockoutDurationMinutes);
}
