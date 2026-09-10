namespace MindCheck.Api.Auth;

// Backs the unified login for both Admin and User roles — see ADR 0014,
// which supersedes ADR 0012's single-shared-password design.
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string JwtSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MindCheck";
    public string Audience { get; set; } = "MindCheck.Client";
    public int TokenLifetimeMinutes { get; set; } = 480;

    // Used once, at startup, to create the very first Admin account when the
    // Users table is empty. After that, admins are managed entirely through
    // AdminUsersController — these values are never consulted again.
    public string BootstrapAdminUsername { get; set; } = string.Empty;
    public string BootstrapAdminEmail { get; set; } = string.Empty;
    public string BootstrapAdminPassword { get; set; } = string.Empty;
}
