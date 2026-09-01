# ADR 0006: Infrastructure DI takes IConfiguration, not a resolved string

**Status:** Accepted

**Context:** `Program.cs` originally called `builder.Configuration.GetConnectionString(...)`
once and passed the resulting string into `AddMindCheckInfrastructure`. Integration
tests using `WebApplicationFactory<Program>` inject an override connection string
(a Testcontainers Postgres) via `ConfigureAppConfiguration`, but that override is
merged into the config pipeline around `Build()` time — after the eager read had
already captured the old appsettings.json value. Tests connected to whatever
Postgres happened to be on `localhost:5432` instead of the test container.

**Decision:** `AddMindCheckInfrastructure(IConfiguration)` stores the live
`IConfiguration`/`ConfigurationManager` reference and only calls
`GetConnectionString("MindCheck")` inside the `AddDbContext` options callback,
which runs lazily the first time a `MindCheckDbContext` is resolved from DI —
by then the host (and any test overrides) is fully built.

**Consequence:** Any config value a test needs to override must be read lazily
(inside a DI callback), never captured into a local variable during `Program.cs`
setup, or `WebApplicationFactory` overrides will silently be ignored.
