# ADR 0009: JWT bearer options bound lazily, same lesson as ADR 0006

**Status:** Accepted

**Context:** `AddJwtBearer(options => { options.TokenValidationParameters = ... })`
originally read `AdminAuthOptions` via `builder.Configuration...Get<AdminAuthOptions>()`
directly in `Program.cs`, before `Build()`. In the Phase 4 integration test,
`WebApplicationFactory`'s config override (a test-only JWT signing key) merges
in around `Build()` time — after that eager read already happened. The admin
login endpoint (which resolves `AdminAuthOptions` via `IOptions<T>`, correctly
lazily) signed tokens with the *test* key, while the bearer handler validated
them against the *stale* appsettings.json key. Every authenticated request
after login failed with 401.

**Decision:** Configure `JwtBearerOptions` through
`AddOptions<JwtBearerOptions>(...).Configure<IOptions<AdminAuthOptions>>(...)`
so `AdminAuthOptions` is resolved the same lazy way the login endpoint already
does — at first use, not at startup.

**Consequence:** Any config value gated by `WebApplicationFactory` overrides
must be consumed through `IOptions<T>` (or another lazy accessor), never read
directly off `builder.Configuration` before `Build()`. This is the same rule
ADR 0006 already established for the connection string; it applies to every
options type, not just that one.
