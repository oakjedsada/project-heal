import { createContext, useContext, type ReactNode } from 'react'
import { useAssessmentSession } from './useAssessmentSession'

type SessionContextValue = ReturnType<typeof useAssessmentSession>

const SessionContext = createContext<SessionContextValue | null>(null)

// A single instance of useAssessmentSession is shared across every page via
// this provider. Each page calling the hook independently would re-run its
// mount-time "rehydrate from localStorage" effect on every SPA navigation
// (Consent -> Assessment, etc.), racing that reconciliation fetch against
// whatever the user just did and occasionally clobbering fresh progress with
// stale data. One shared instance means rehydration only ever runs once per
// real browser session (a true page load), which is the resilience the
// feature is actually meant to provide.
export function SessionProvider({ children }: { children: ReactNode }) {
  const value = useAssessmentSession()
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}

export function useSession(): SessionContextValue {
  const ctx = useContext(SessionContext)
  if (!ctx) {
    throw new Error('useSession must be used within a SessionProvider')
  }
  return ctx
}
