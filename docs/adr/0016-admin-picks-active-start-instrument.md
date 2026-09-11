# ADR 0016: Admin picks the active starting instrument, one at a time

**Status:** Accepted

**Context:** `StartSessionUseCase` has always picked a session's first instrument
from whichever `flow_transitions` row has `from_instrument_id = null`,
ordered by id and taking the first — but there was no admin-facing way to
*choose* that row. To point new sessions at a different instrument, an admin
had to manually delete the existing start transition and hand-craft a new
one on the "เส้นทางแบบประเมิน" (flow transitions) page — easy to get wrong
(e.g. leaving two start rows, silently making the older one win forever).

**Decision:** New `PATCH /api/admin/instruments/{id}/activate` +
`SetActiveInstrumentUseCase` do the delete-old/insert-new as one atomic
step (`IFlowTransitionRepository.SetStartTransitionAsync`, wrapped in a DB
transaction like `CreateFullInstrumentAsync` already does for instrument
creation). This makes "exactly one active start transition" an invariant
the system itself now maintains, not something the admin has to get right
by hand. `InstrumentSummaryDto` gained `IsActiveStart` so the UI can show
which one is currently active without a second round-trip. New admin page
"จัดการแบบสอบถาม" (`/admin/instruments`) lists every instrument with a
"ตั้งเป็นแบบสอบถามที่ใช้งาน" button for the inactive ones.

**Consequence:** This only changes the *entry point* of the flow graph —
the cascade after it (2Q→9Q→8Q, ScoreLevelEquals/QuestionScoreAtLeast
transitions) is untouched and still hand-wired via the existing flow
transitions page. Switching the active instrument doesn't delete or
disable any other flow transitions in the graph, including ones that are
no longer reachable from the new start — that's still on the admin to
notice (see the existing "nothing validates the flow graph" known gap).
