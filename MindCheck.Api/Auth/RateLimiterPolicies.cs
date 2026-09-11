namespace MindCheck.Api.Auth;

public static class RateLimiterPolicies
{
    // Applied to the unauthenticated /api/auth/* endpoints (register, login,
    // forgot/reset-password) — the only ones an attacker can hammer without
    // already holding a valid token.
    public const string Auth = "auth";
}
