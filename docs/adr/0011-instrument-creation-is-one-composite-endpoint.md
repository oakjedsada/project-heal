# ADR 0011: Instrument creation is one composite, transactional endpoint

**Status:** Accepted

**Context:** A new instrument needs questions, choices, scoring rules, and
(optionally) risk rules to be minimally usable. Risk rules reference "which
question" by threshold logic, but the question has no id yet at request time.

**Decision:** `POST /api/admin/instruments` takes the whole instrument in one
request body; risk rules reference questions by the question's `orderNo`
*within that same request*, not a database id. `IInstrumentRepository.
CreateFullInstrumentAsync` saves the instrument+questions+choices graph first
(so EF assigns real ids), builds `ScoringRule`/`RiskRule` Domain objects from
plain specs using those now-real ids, then saves those — both steps wrapped
in one DB transaction (see `ScoringRuleSpec`/`RiskRuleSpec` in
`Application/Abstractions`).

**Consequence:** Creating an instrument is all-or-nothing — a bad risk rule
reference (an orderNo not present in the request) fails the whole request
before anything is written, never leaving a half-created instrument. The
admin UI's form maps directly onto one request shape instead of orchestrating
several sequential calls itself.
