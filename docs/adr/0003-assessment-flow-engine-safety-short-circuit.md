# ADR 0003: AssessmentFlowEngine is a pure domain service; risk always short-circuits

**Status:** Accepted

**Context:** The flow between instruments (ST-5 -> 2Q -> 9Q -> 8Q) must be data-driven,
but the safety path (risk_rules firing) must never be skippable by flow_transitions data.

**Decision:** `IAssessmentFlowEngine.Decide(...)` takes already-evaluated
`RiskEvaluationResult` + `ScoringResult` + `Answer[]` + the `FlowTransition` rows for the
instrument just completed, and does no I/O. It checks `riskResult.IsEscalated` FIRST and
returns `FlowOutcome.Emergency` unconditionally before ever consulting flow_transitions.
Only if not escalated does it match `FlowTransition` rows by `ConditionType`
(`Always` / `ScoreLevelEquals` / `QuestionScoreAtLeast`) — never by instrument name.

**Consequence:** No instrument-specific branching exists in code. A bad or missing
flow_transitions row can misroute the flow, but can never suppress an emergency.
