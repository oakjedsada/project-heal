namespace MindCheck.Api.Auth;

// Deliberately minimal: one shared password, one role. This gates the admin
// UI against casual visitors for a portfolio demo — it is not meant to
// withstand a real attacker (no user table, no hashing, no refresh tokens).
public sealed class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";

    public string Password { get; set; } = string.Empty;
    public string JwtSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MindCheck";
    public string Audience { get; set; } = "MindCheck.Admin";
    public int TokenLifetimeMinutes { get; set; } = 480;
}
