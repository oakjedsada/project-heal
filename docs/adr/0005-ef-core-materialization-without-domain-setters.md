# ADR 0005: EF Core materializes Domain entities without exposing setters

**Status:** Accepted

**Context:** EF Core cannot bind collection navigations (`Instrument.Questions`,
`Question.Choices`) or owned-type references (`ScoringRule.Range`,
`RiskRule.Condition`) through a constructor parameter — only scalar/FK
properties. Domain entities are otherwise immutable (get-only properties, one
public constructor), which is exactly what conflicts here.

**Decision:** Each affected entity gets a second, private constructor with only
the constructor-bindable scalar parameters; the public constructor delegates to
it and then sets the backing field directly. Infrastructure configures those
navigations with `UsePropertyAccessMode(PropertyAccessMode.Field)` so EF writes
straight to the field after calling the private constructor. No entity gains a
public setter, and Domain still has zero reference to EF Core.

**Consequence:** Materialization is reflection-based on private members, which
is more "magic" than a plain settable property, but keeps the Domain API
honestly immutable for every other caller. Verified against a real Postgres
container: `dotnet ef migrations add` + `database update` both succeed and seed
data (4 instruments, 24 questions, 92 choices, 11 scoring rules, 1 risk rule, 4
flow transitions) round-trips correctly.
