# ADR 0015: Account lockout, auth rate limiting, and revocable JWTs

**Status:** Accepted

**Context:** ADR 0014 gave every user a real password-based account but left
three gaps explicitly flagged as "fine for a demo, not for a real deploy":
no limit on repeated login attempts, no rate limit on the unauthenticated
`/api/auth/*` endpoints, and no way to invalidate a JWT once issued (it's
valid until its 8-hour expiry no matter what). The user asked to close these
before considering the app deployable with real accounts.

**Decision:** Three independent, additive mechanisms, none requiring new
infrastructure:
- **Lockout** — `User` gains `FailedLoginAttempts`/`LockedOutUntil`.
  `LoginUseCase` checks `IsLockedOut` before verifying the password (locked
  wins even over a correct password — the attempt count already narrowed
  down that the account exists, so there's no enumeration risk left to
  protect by staying silent); a wrong password increments the counter via
  `RegisterFailedLogin` and locks out for `Auth:LockoutDurationMinutes` once
  `Auth:MaxFailedLoginAttempts` is hit (defaults 5 / 15). New
  `AccountLockedOutException` maps to 423.
- **Rate limiting** — ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting`
  (no new package), a fixed-window policy keyed by caller IP, applied via
  `[EnableRateLimiting("auth")]` to register/login/forgot-password/reset-password
  only — the endpoints reachable without a token. Limits
  (`Auth:AuthRateLimitPermitLimit`/`AuthRateLimitWindowSeconds`, default
  10/60s) are read from `IOptions<AuthOptions>` inside the policy factory
  itself (per-request, not at startup) to keep the same lazy-binding
  discipline as ADR 0006/0009.
- **Revocable JWT** — `User.TokenVersion` (bumped by `ChangePassword` and by
  a new self-service `POST /api/auth/logout-all`) is embedded as a `tv`
  claim at token-issue time. `JwtBearerOptions.Events.OnTokenValidated`
  looks the user back up per request and fails validation if the claim no
  longer matches the DB value. This trades a DB read on every authenticated
  request for not needing a token blacklist store or refresh-token
  rotation — acceptable at this app's scale, and it means a password
  change (self-service or admin-edited) now also kills every session that
  isn't the one making the change, which wasn't true before.

**Consequence:** `AddAccountSecurityFieldsToUsers` adds three columns, all
with defaults, so no volume wipe is needed. `IAuthTokenGenerator.GenerateToken`
and `IUserRepository` callers all gained a parameter/round-trip; every call
site was updated. Rate limiting and lockout share one HTTP client's IP as
the partition key, so multiple users behind the same NAT/proxy share one
budget — acceptable for a portfolio demo, worth revisiting (e.g. a
per-account counter in addition) before any real multi-tenant deployment.
