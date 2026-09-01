# ADR 0001: Layered architecture with one-directional dependencies

**Status:** Accepted

**Context:** MindCheck must demonstrate a layered architecture where dependencies
point one way, and business rules stay testable without infrastructure.

**Decision:** Domain has zero project references (not even EF Core). Application
depends only on Domain and defines repository interfaces. Infrastructure depends
on Application + Domain and implements those interfaces. Api depends on all three
and wires DI. Direction: Api -> Infrastructure -> Application -> Domain.

**Consequence:** Domain and its tests can build/run without a database, ASP.NET,
or any package beyond the BCL.
