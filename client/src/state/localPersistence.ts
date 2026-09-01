import type { QuestionDto } from '../api/types'

export interface HistoryEntry {
  question: QuestionDto
  choiceId: number | null
  /** false once the backend has rejected an edit (its instrument already finalized). */
  editable: boolean
}

const SESSION_ID_KEY = 'mindcheck.sessionId'
const HISTORY_KEY = 'mindcheck.history'

// localStorage can throw (private browsing, storage disabled) — every call
// here is a best-effort cache, never the source of truth, so failures are
// swallowed rather than surfaced to the user.
export function loadSessionId(): string | null {
  try {
    return window.localStorage.getItem(SESSION_ID_KEY)
  } catch {
    return null
  }
}

export function saveSessionId(sessionId: string): void {
  try {
    window.localStorage.setItem(SESSION_ID_KEY, sessionId)
  } catch {
    // ignore
  }
}

export function loadHistory(): HistoryEntry[] {
  try {
    const raw = window.localStorage.getItem(HISTORY_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw) as HistoryEntry[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

export function saveHistory(history: HistoryEntry[]): void {
  try {
    window.localStorage.setItem(HISTORY_KEY, JSON.stringify(history))
  } catch {
    // ignore
  }
}

export function clearStoredSession(): void {
  try {
    window.localStorage.removeItem(SESSION_ID_KEY)
    window.localStorage.removeItem(HISTORY_KEY)
  } catch {
    // ignore
  }
}
