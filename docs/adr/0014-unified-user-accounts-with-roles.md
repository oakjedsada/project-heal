# ADR 0014: Real user accounts, unified into one Users table with a Role

**Status:** Accepted (supersedes ADR 0012)

**Context:** The product decision changed: people taking the assessment must
now register/log in with a real account instead of an anonymous session, and
there must be exactly two roles — `Admin` (today's back-office power user)
and `User` (someone taking the assessment) — with an admin UI to add/remove
users and change roles. Running that alongside the old single-shared-password
admin login would mean two disconnected auth systems for one concept
("identity").

**Decision:** One `users` table (`Id`, `Username`, `PasswordHash`, `Role`,
`CreatedAt`) backs both account kinds. `POST /api/auth/register` always
assigns `UserRole.User` (self-registration can never mint an Admin);
`POST /api/auth/login` returns a JWT with `NameIdentifier`/`Name`/`Role`
claims for either role, replacing the old admin-only
`POST /api/admin/auth/login`. `SessionsController` requires `[Authorize]`
(any role) and every session use case now checks `session.UserId ==
callerUserId`, throwing `SessionAccessDeniedException` (403) otherwise — a
real authorization check that didn't exist before (the old anonymous flow's
only protection was knowing the session GUID). Every `/api/admin/*`
controller requires `[Authorize(Roles = "Admin")]`, not just `[Authorize]` —
role-blind admin authorization was safe when the only JWT issuer was the
admin password login, but became a privilege-escalation hole the moment a
`User`-role account could also get a JWT. Passwords are hashed via
`Microsoft.Extensions.Identity.Core`'s `PasswordHasher<T>` (`MindCheck.Api/Auth/PasswordHasherAdapter.cs`), behind an `IPasswordHasher` interface owned
by `MindCheck.Application` (mirrors how `IHelpResourceProvider` is split)
since `MindCheck.Application.csproj` has zero NuGet dependencies. Because the
shared admin password is gone, the very first `Admin` account is seeded once
at startup from `Auth:BootstrapAdminUsername`/`Auth:BootstrapAdminPassword`
config, only when the `users` table is empty — after that, admins are
created/promoted/demoted/deleted entirely through `AdminUsersController`.

**Consequence:** `sessions.user_id` is a required column with no backfill
path for pre-existing anonymous session rows, so this migration requires
wiping the local dev database. The app's prior "no need to reveal your
identity" pitch is gone by design — `ConsentPage`'s copy and the README were
updated to stop claiming anonymity. `Session.AnonToken` is now doubly dead
code (it was already unused before this change) — left in place, flagged as
a follow-up cleanup, not touched here.
