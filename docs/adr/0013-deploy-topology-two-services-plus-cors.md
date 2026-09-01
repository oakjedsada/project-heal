# ADR 0013: Deploy as two independent services with CORS, not one combined origin

**Status:** Accepted

**Context:** Client and Api needed to be deployable separately (e.g. two
Railway services), each with their own Dockerfile, rather than the Api
serving the client's static files itself.

**Decision:** `client/src/api/client.ts` reads `VITE_API_BASE_URL` (a Vite
build-time env var, empty by default so local dev keeps using the same-origin
Vite proxy). The Api gets a `Cors` config section and only enables specific
allowed origins in `Program.cs` — empty by default, so local dev is
unaffected. Both Dockerfiles read `$PORT` at container start (Railway's
convention) rather than hardcoding a port: the Api via a shell-form
`ENTRYPOINT` setting `ASPNETCORE_URLS`, the client's nginx via the official
image's `/etc/nginx/templates/*.template` + `envsubst` mechanism, restricted
to substituting `${PORT}` only (`NGINX_ENVSUBST_FILTER=PORT`) so nginx's own
`$uri`/`$host` template variables aren't mangled by the substitution pass.

**Consequence:** Verified with a full `docker compose up --build` run — real
cross-origin `fetch` calls, real CORS preflight, sample-to-emergency flow, and
nginx's SPA fallback on a hard-refreshed deep route (`/admin/login`) — all
against real containers, not mocks. Railway deploy still needs the two
services wired to each other's URLs after first deploy (documented in
`DEPLOY.md`), since neither URL exists before that.
