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

    // The client's own origin, used only to build the link inside a
    // password-reset email/response — the API has no other reason to know
    // where the frontend lives.
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
    public int PasswordResetTokenLifetimeMinutes { get; set; } = 60;

    // Used once, at startup, to create the very first Admin account when the
    // Users table is empty. After that, admins are managed entirely through
    // AdminUsersController — these values are never consulted again.
    public string BootstrapAdminUsername { get; set; } = string.Empty;
    public string BootstrapAdminEmail { get; set; } = string.Empty;
    public string BootstrapAdminPassword { get; set; } = string.Empty;

    // Account lockout — see ADR 0015.
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;

    // Rate limit applied to the unauthenticated /api/auth/* endpoints, per
    // caller IP — see ADR 0015.
    public int AuthRateLimitPermitLimit { get; set; } = 10;
    public int AuthRateLimitWindowSeconds { get; set; } = 60;
}
