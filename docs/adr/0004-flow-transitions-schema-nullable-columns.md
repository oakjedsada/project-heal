# ADR 0004: flow_transitions gets nullable from_instrument_id and question_id

**Status:** Accepted

**Context:** Two conditions need modeling: "total score falls in level X" (2Q -> 9Q)
and "one specific question's answer is at/above a threshold" (9Q item 9 -> 8Q). A third
implicit condition is "session just started" (nothing completed yet -> ST-5).

**Decision:** `from_instrument_id` is nullable — a null row is the session-start
transition. `question_id` is nullable and added as its own column (not encoded into
`condition_value`) so `QuestionScoreAtLeast` doesn't need string-parsing a compound key.

**Consequence:** Even the entry instrument is chosen by a DB row, not a code constant.
Adding a new instrument or changing which question gates a branch is a data change only.
