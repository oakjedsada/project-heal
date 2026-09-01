# ADR 0007: One shared session hook instance via Context

**Status:** Accepted

**Context:** `useAssessmentSession` rehydrates from localStorage on mount by
calling `GET /next` and reconciling the result against cached history. When
`ConsentPage` and `AssessmentPage` each called the hook independently, that
mount effect re-ran on every SPA navigation between them (not just on a real
page reload), and its async resolution raced against the user's own in-flight
actions — a late-resolving reconciliation fetch could overwrite freshly
advanced progress with stale data. Reproduced live: sequential answers got
stuck re-showing the same question after a few steps.

**Decision:** A single `SessionProvider` (`src/state/SessionContext.tsx`)
owns one `useAssessmentSession()` instance for the whole app; every page reads
it via `useSession()`. Rehydration now only runs once per actual browser
session, matching what it's meant to protect against (a closed tab / reload),
not every internal route change.

**Consequence:** Any future page added to the flow must consume `useSession()`
rather than calling the hook directly, or the same race reappears.
