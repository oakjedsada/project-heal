# ADR 0002: Evaluators consume flattened value objects, not aggregates

**Status:** Accepted

**Context:** `IScoringEvaluator`/`IRiskEvaluator` need answers + rules, but building
a full `Instrument` -> `Question` -> `Choice` graph just to unit-test scoring math
is unnecessary ceremony.

**Decision:** `Answer` carries a pre-resolved `Score` (`QuestionId`, `ChoiceId`,
`Score`). Resolving `Choice.Score` into an `Answer` is an Application-layer
concern; evaluators only see flat `Answer`/`ScoringRule`/`RiskRule` collections.

**Consequence:** Evaluator tests build small fixtures directly, no aggregate
construction required. Application layer owns the resolution step (Phase 2+).
