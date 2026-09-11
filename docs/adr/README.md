# Architecture Decision Records

Short-form ADRs (≤15 lines each), in the order the decisions were actually
made across the project's five build phases. Each one names the trade-off and
the reason, not just the outcome — read them if you want to know *why*
something is shaped the way it is, not just what it is.

| # | Decision | Phase |
|---|---|---|
| [0001](0001-layered-architecture-dependency-direction.md) | Layered architecture, dependencies point one way (Api → Infrastructure → Application → Domain) | 1 |
| [0002](0002-evaluators-use-flattened-value-objects.md) | Scoring/risk evaluators take flat value objects, not full aggregates | 1 |
| [0003](0003-assessment-flow-engine-safety-short-circuit.md) | Flow engine is a pure function; risk escalation always short-circuits flow_transitions | 2 |
| [0004](0004-flow-transitions-schema-nullable-columns.md) | `flow_transitions` gets nullable `from_instrument_id` and `question_id` | 2 |
| [0005](0005-ef-core-materialization-without-domain-setters.md) | EF Core materializes Domain entities without exposing public setters | 2 |
| [0006](0006-defer-connection-string-resolution.md) | Connection string must be read lazily, not eagerly in `Program.cs` | 2 |
| [0007](0007-client-shares-one-session-hook-via-context.md) | Client shares one session hook instance via React Context | 3 |
| [0008](0008-client-back-nav-relies-on-backend-validation.md) | Client back-navigation defers to the backend's existing validation | 3 |
| [0009](0009-jwt-options-bound-lazily.md) | JWT bearer options bound lazily — same lesson as 0006, different feature | 4 |
| [0010](0010-reference-ids-become-identity-columns.md) | Reference table ids converted to identity columns for runtime admin inserts | 4 |
| [0011](0011-instrument-creation-is-one-composite-endpoint.md) | Instrument creation is one composite, transactional endpoint | 4 |
| [0012](0012-admin-auth-single-password-jwt.md) | Admin auth is a single shared password issuing a JWT (demo-grade, by design) | 4 |
| [0013](0013-deploy-topology-two-services-plus-cors.md) | Deploy as two independent services (client + api) with CORS | 4 |
| [0014](0014-unified-user-accounts-with-roles.md) | Real user accounts, unified into one Users table with a Role (supersedes 0012) | 5 |
| [0015](0015-auth-hardening-lockout-rate-limit-revocable-jwt.md) | Account lockout, auth rate limiting, and revocable JWTs | 5 |

Two lessons repeat on purpose (0006 and 0009): the same "config read too early"
mistake happened twice, for two different features, months apart in the
build. That repetition is itself useful signal — see 0009 for the second
occurrence.
