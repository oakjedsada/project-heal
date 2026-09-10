using Microsoft.Extensions.Options;
using MindCheck.Application.Abstractions;

namespace MindCheck.Api.Auth;

public sealed class PasswordResetLinkBuilder : IPasswordResetLinkBuilder
{
    private readonly AuthOptions _options;

    public PasswordResetLinkBuilder(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public string BuildResetLink(string token) =>
        $"{_options.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

    public TimeSpan TokenLifetime => TimeSpan.FromMinutes(_options.PasswordResetTokenLifetimeMinutes);
}
