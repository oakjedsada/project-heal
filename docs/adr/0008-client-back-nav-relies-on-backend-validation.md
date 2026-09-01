# ADR 0008: Client back-navigation defers to the backend's own validation

**Status:** Accepted

**Context:** The UI needs to let a user revisit and edit a recent answer, but
the backend only recomputes an instrument's score the moment its last
question is answered — editing an answer whose instrument has already been
scored must not silently go nowhere. The API has no endpoint exposing
"which instrument is this question part of" or instrument boundaries.

**Decision:** The client keeps one continuous, ungrouped history of every
question shown in the session (not scoped per instrument) and lets the user
step back/forward through all of it locally, no API call. Attempting to
change an old answer always re-POSTs to `/answers`; if the backend rejects it
(`400`, because `SubmitAnswerUseCase` validates the question against the
*current* instrument), the entry is marked `editable: false` and the UI shows
"can't be changed anymore." The client never tries to know instrument
boundaries itself.

**Consequence:** No backend change was needed this phase. The UX degrades
gracefully (a rejected edit is explained, not silently dropped) instead of
requiring the frontend to hardcode or fetch instrument structure.
