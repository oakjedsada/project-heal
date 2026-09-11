# ADR 0017: Instrument editing, gated by referential-safety checks

**Status:** Accepted

**Context:** Admin could create an instrument (ADR 0011) and pick which one
is active (ADR 0016), but never edit one afterward — every Domain entity in
the graph (`Instrument`, `Question`, `Choice`, `ScoringRule`) was fully
immutable (`{ get; }`-only properties, no setters at all), so even fixing a
typo meant creating a whole new instrument. The risk in allowing edits isn't
hypothetical: `Response` rows reference `QuestionId`/`ChoiceId` with no FK
constraint (existing convention) and `SubmitAnswerUseCase` does a *live*
`Choice.Score` lookup when completing an instrument — deleting a
question/choice a real session already answered can 500 that session outright,
not just look wrong. Separately, `GetResultUseCase` resolves a completed
`Result`'s interpretation/advice by matching its frozen `Level` string
against the *current* `ScoringRule` rows every time a result page is viewed
(not a snapshot) — renaming or deleting the scoring rule for a level a past
`Result` used silently blanks that text for anyone revisiting it.

**Decision:** `Instrument`/`Question`/`Choice`/`ScoringRule` gained private
setters and mutator methods (`ChangeDetails`, `AddQuestion`/`RemoveQuestion`,
`AddChoice`/`RemoveChoice`), mirroring `User`'s existing mutable-entity
pattern. One composite `PUT /api/admin/instruments/{id}`
(`UpdateInstrumentUseCase`) accepts the *whole* desired graph — each
question/choice/scoring-rule optionally carries its existing id (`null` id =
new row; an existing id missing from the payload = deletion). Before
touching anything, the use case computes the add/update/delete diff and
runs two checks: `IResponseRepository.GetAnsweredQuestionIdsAsync`/
`GetAnsweredChoiceIdsAsync` block deleting anything a real answer
references (also checks `FlowTransition.QuestionId` for the same reason),
and `IResultRepository.GetLevelsInUseAsync` blocks removing or renaming a
scoring level any existing `Result` used. Both throw
`InvalidAdminRequestException` (400) *before* any write — a rejected edit
leaves the instrument completely untouched, not partially applied.
`IInstrumentRepository.UpdateFullInstrumentAsync` then applies the change
by loading the instrument *tracked* (not `AsNoTracking`, unlike every other
read here) and calling the same domain mutators the use case already
validated against — EF's change tracker turns collection
adds/removes/edits into the right INSERT/UPDATE/DELETE automatically, one
transaction, same pattern `CreateFullInstrumentAsync` already established
for the initial insert. Risk rules are deliberately not editable through
this endpoint (out of scope for this pass); a new `GetInstrumentFullDetailUseCase`
+ `InstrumentFullDetailDto` (choices and scoring rules included, unlike the
existing minimal `InstrumentDetailDto` the flow-transitions question picker
uses) feeds the new "จัดการแบบสอบถาม" list's "แก้ไข" link into a new
`EditInstrumentPage`.

**Consequence:** Editing in place (text/label/score edits, not deletions)
carries no referential risk at all — the row id never changes. Editing a
scoring rule's *interpretation/advice* text while keeping its *level*
string retroactively changes what a past `Result` displays, which is a
known, accepted characteristic of `GetResultUseCase`'s live lookup — not
new here, just now reachable via the UI instead of only direct DB edits.
Risk-rule deletion safety was intentionally not built (they're immutable
via this endpoint), so an instrument's risk rules can still only be set at
creation time.
