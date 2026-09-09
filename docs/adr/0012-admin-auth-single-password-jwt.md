# ADR 0012: Admin auth is a single shared password issuing a JWT

**Status:** Superseded by [ADR 0014](0014-unified-user-accounts-with-roles.md)

**Context:** The admin area only needs to keep casual visitors out of a
portfolio demo — not withstand a real attacker. No user accounts or roles
beyond "is admin" exist anywhere else in the system.

**Decision:** One password lives in config (`AdminAuth:Password`, plaintext —
explicitly a demo-grade simplification, not production security).
`POST /api/admin/auth/login` checks it and issues a short-lived JWT (HMAC
SHA-256, `role: admin` claim, default 8h expiry). Every `/api/admin/*`
controller carries `[Authorize]`; there is no user table, no refresh token,
no per-user anything. The client stores the token in `sessionStorage` (not
`localStorage`, unlike the public assessment's resume token) so it doesn't
outlive the tab.

**Consequence:** Anyone with the password has full admin access — acceptable
here, would not be for a real deployment. `JwtBearerOptions` must be bound via
`IOptions<AdminAuthOptions>` lazily, not read eagerly in `Program.cs` — see
ADR 0009, which was discovered because of this exact feature.
