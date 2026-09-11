# Running & deploying MindCheck

## Local, full stack, via Docker

```bash
docker compose up --build
```

Starts three containers: `db` (Postgres 16), `api` (ASP.NET Core, auto-migrates
on startup), `client` (Vite build served by nginx). Verified working end to
end, including the sample-data button reaching the emergency page.

- Client: http://localhost:18081
- Api + Swagger: http://localhost:18080/swagger
- Bootstrap admin login (this compose file only): username `admin`, password `local-docker-admin-password` — used once to seed the first admin account; manage further admins via the in-app user management page afterward

`db` alone (for local `dotnet run` against a real Postgres instead of the full
container stack) is still `docker compose up -d db`, unchanged from earlier
phases — it listens on host port 5433.

## Deploying to Railway (two services + one Postgres plugin)

Railway needs your account; this is written so you can follow it yourself —
these steps can't be done on your behalf without your login.

1. **Postgres**: In your Railway project, add a Postgres plugin. Railway
   gives it connection variables (`PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`,
   `PGPASSWORD`) you'll reference from the API service.

2. **Api service**: New service → deploy from this repo → build method
   "Dockerfile", path `MindCheck.Api/Dockerfile`, root directory `/` (the
   Dockerfile needs the repo root as build context — it copies sibling
   projects). Set these variables:
   - `ConnectionStrings__MindCheck` = `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}`
   - `Auth__JwtSigningKey` = a random string, 32+ bytes
   - `Auth__BootstrapAdminUsername` / `Auth__BootstrapAdminEmail` / `Auth__BootstrapAdminPassword` = a real username, email, and password, not the dev placeholders — only used once, to seed the first admin account when the `users` table is empty
   - `Auth__FrontendBaseUrl` = the client service's public URL (see step 4) — used to build the link inside password-reset emails
   - `Cors__AllowedOrigins__0` = the client service's public URL (same value as above)
   - `Smtp__Host` / `Smtp__Port` / `Smtp__Username` / `Smtp__Password` / `Smtp__FromAddress` = real SMTP credentials (e.g. a Gmail address + App Password — see `.env.example`) so "ลืมรหัสผ่าน" emails real users instead of returning the reset link in the API response
   - `EmergencyHelpResources__0__Label` / `__Contact` = real crisis line info before this is shown to anyone for real

   Railway injects `PORT` automatically; the Dockerfile already listens on it.

3. **Client service**: New service → same repo → build method "Dockerfile",
   path `client/Dockerfile`, root directory `client`. Set:
   - Build arg `VITE_API_BASE_URL` = the Api service's public URL (step 2)

4. **Chicken-and-egg**: Railway only assigns a public URL after a service's
   first deploy, so the exact order above (Postgres → Api → Client) means
   step 2's `Cors__AllowedOrigins__0` won't be known until step 3 exists.
   Deploy Api first without it, get the client's URL, come back and set it,
   redeploy Api. Same idea in reverse if you'd rather fix the Api's URL first.

## What's still a placeholder, deploy-blocking for a real (non-demo) launch

- Every scoring cut-off and risk threshold in `MindCheck.Infrastructure/Seed/SeedData.cs`
  is dummy data (`PLACEHOLDER-DO-NOT-USE-CLINICALLY`) — replace via the admin
  UI or a new seed before this represents anything real.
- Real per-user accounts with roles now exist (see [ADR 0014](docs/adr/0014-unified-user-accounts-with-roles.md), supersedes [ADR 0012](docs/adr/0012-admin-auth-single-password-jwt.md)); login lockout, auth rate limiting, and revocable JWTs were added on top (see [ADR 0015](docs/adr/0015-auth-hardening-lockout-rate-limit-revocable-jwt.md)). Still worth reviewing before a real deploy: the lockout/rate-limit counters are per-IP, so users behind the same NAT share one budget, and `Auth:MaxFailedLoginAttempts`/`Auth:AuthRateLimitPermitLimit` should be tuned for expected real traffic rather than left at the demo defaults.
