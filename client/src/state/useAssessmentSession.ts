import { useCallback, useEffect, useRef, useState } from 'react'
import { apiClient } from '../api/client'
import type { QuestionDto, ResultResponse } from '../api/types'
import {
  clearStoredSession,
  loadHistory,
  loadSessionId,
  saveHistory,
  saveSessionId,
  type HistoryEntry,
} from './localPersistence'

export type SubmitOutcome =
  | { kind: 'advanced' }
  | { kind: 'finished'; nextAction: string }
  | { kind: 'rejected' }
  | { kind: 'error'; message: string }

interface AdvanceResult {
  kind: 'question' | 'complete'
  question?: QuestionDto
}

export function useAssessmentSession(authHeader: Record<string, string>) {
  const [sessionId, setSessionIdState] = useState<string | null>(() => loadSessionId())
  const [history, setHistory] = useState<HistoryEntry[]>(() => loadHistory())
  const [viewIndex, setViewIndexState] = useState(() => Math.max(0, loadHistory().length - 1))
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // React state only lands on the next render, but runSampleAutoFill drives a
  // tight async loop that must see each step's result immediately — so every
  // mutable piece of flow state (history, viewIndex, sessionId) is mirrored in
  // a ref that's written synchronously at the same time as the state setter.
  const historyRef = useRef(history)
  const viewIndexRef = useRef(viewIndex)
  const sessionIdRef = useRef(sessionId)

  const setSessionId = useCallback((sid: string | null) => {
    sessionIdRef.current = sid
    setSessionIdState(sid)
  }, [])

  const setViewIndex = useCallback((index: number) => {
    viewIndexRef.current = index
    setViewIndexState(index)
  }, [])

  const commitHistory = useCallback((next: HistoryEntry[]) => {
    historyRef.current = next
    setHistory(next)
    saveHistory(next)
  }, [])

  const fetchNext = useCallback(async (sid: string): Promise<AdvanceResult> => {
    const { data, error: apiError } = await apiClient.GET('/api/sessions/{id}/next', {
      params: { path: { id: sid } },
      headers: authHeader,
    })
    if (apiError || !data) {
      throw new Error('ไม่สามารถโหลดคำถามถัดไปได้')
    }
    if (data.isComplete || !data.question) {
      return { kind: 'complete' }
    }
    return { kind: 'question', question: data.question }
  }, [authHeader])

  const fetchResult = useCallback(async (sid: string): Promise<ResultResponse> => {
    const { data, error: apiError } = await apiClient.GET('/api/sessions/{id}/result', {
      params: { path: { id: sid } },
      headers: authHeader,
    })
    if (apiError || !data) {
      throw new Error('ไม่สามารถโหลดผลลัพธ์ได้')
    }
    return data
  }, [authHeader])

  /** Rehydrates from localStorage on mount by reconciling with the server's view of "next". */
  useEffect(() => {
    const sid = loadSessionId()
    if (!sid) return

    let cancelled = false
    setIsLoading(true)
    setError(null)

    fetchNext(sid)
      .then((result) => {
        if (cancelled) return
        if (result.kind === 'complete') {
          return
        }
        const cached = loadHistory()
        const tail = cached[cached.length - 1]
        if (tail && tail.question.questionId === result.question!.questionId) {
          historyRef.current = cached
          setHistory(cached)
          setViewIndex(cached.length - 1)
        } else {
          const fresh = [{ question: result.question!, choiceId: null, editable: true }]
          commitHistory(fresh)
          setViewIndex(0)
        }
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'เกิดข้อผิดพลาด')
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const startSession = useCallback(async (): Promise<void> => {
    setIsLoading(true)
    setError(null)
    try {
      const { data, error: apiError } = await apiClient.POST('/api/sessions', { headers: authHeader })
      if (apiError || !data) {
        throw new Error('ไม่สามารถเริ่มเซสชันได้')
      }
      const sid = data.sessionId!
      saveSessionId(sid)
      setSessionId(sid)

      const result = await fetchNext(sid)
      if (result.kind === 'complete') {
        commitHistory([])
        setViewIndex(0)
        return
      }
      commitHistory([{ question: result.question!, choiceId: null, editable: true }])
      setViewIndex(0)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'เกิดข้อผิดพลาด')
      throw e
    } finally {
      setIsLoading(false)
    }
  }, [fetchNext, commitHistory, setSessionId, setViewIndex, authHeader])

  const goBack = useCallback(() => {
    setViewIndex(Math.max(0, viewIndexRef.current - 1))
  }, [setViewIndex])

  const goForward = useCallback(() => {
    setViewIndex(Math.min(historyRef.current.length - 1, viewIndexRef.current + 1))
  }, [setViewIndex])

  const submitChoice = useCallback(
    async (choiceId: number): Promise<SubmitOutcome> => {
      const sid = sessionIdRef.current ?? loadSessionId()
      if (!sid) {
        return { kind: 'error', message: 'ไม่พบเซสชัน' }
      }

      const currentIndex = viewIndexRef.current
      const entry = historyRef.current[currentIndex]
      if (!entry) {
        return { kind: 'error', message: 'ไม่พบคำถามปัจจุบัน' }
      }

      setIsLoading(true)
      setError(null)
      try {
        const { error: apiError, response } = await apiClient.POST('/api/sessions/{id}/answers', {
          params: { path: { id: sid } },
          body: { questionId: entry.question.questionId, choiceId },
          headers: authHeader,
        })

        if (apiError || !response.ok) {
          if (response.status === 400 || response.status === 409) {
            commitHistory(
              historyRef.current.map((item, idx) =>
                idx === currentIndex ? { ...item, editable: false } : item,
              ),
            )
            return { kind: 'rejected' }
          }
          throw new Error('ไม่สามารถบันทึกคำตอบได้')
        }

        const isTail = currentIndex === historyRef.current.length - 1
        const withAnswer = historyRef.current.map((item, idx) =>
          idx === currentIndex ? { ...item, choiceId } : item,
        )

        if (!isTail) {
          commitHistory(withAnswer)
          return { kind: 'advanced' }
        }

        const next = await fetchNext(sid)
        if (next.kind === 'complete') {
          commitHistory(withAnswer)
          const result = await fetchResult(sid)
          return { kind: 'finished', nextAction: result.nextAction ?? 'completed' }
        }

        commitHistory([...withAnswer, { question: next.question!, choiceId: null, editable: true }])
        setViewIndex(currentIndex + 1)
        return { kind: 'advanced' }
      } catch (e) {
        const message = e instanceof Error ? e.message : 'เกิดข้อผิดพลาด'
        setError(message)
        return { kind: 'error', message }
      } finally {
        setIsLoading(false)
      }
    },
    [fetchNext, fetchResult, commitHistory, setViewIndex, authHeader],
  )

  /** Auto-answers every remaining question at the tail, always picking the
   * most severe (last) choice, so a portfolio viewer can reach a result fast. */
  const runSampleAutoFill = useCallback(async (): Promise<SubmitOutcome> => {
    // eslint-disable-next-line no-constant-condition
    while (true) {
      const tail = historyRef.current[historyRef.current.length - 1]
      if (!tail || !tail.question.choices || tail.question.choices.length === 0) {
        return { kind: 'error', message: 'ไม่พบตัวเลือกคำถาม' }
      }
      setViewIndex(historyRef.current.length - 1)
      const mostSevere = tail.question.choices[tail.question.choices.length - 1]
      const outcome = await submitChoice(mostSevere.choiceId!)
      if (outcome.kind !== 'advanced') {
        return outcome
      }
    }
  }, [submitChoice, setViewIndex])

  /** Used when a page mounts with no cached question but a session id still
   * exists locally — e.g. the user reloaded after already finishing. */
  const checkCompletion = useCallback(async (): Promise<ResultResponse | null> => {
    const sid = sessionIdRef.current ?? loadSessionId()
    if (!sid) return null
    try {
      return await fetchResult(sid)
    } catch {
      return null
    }
  }, [fetchResult])

  const resetSession = useCallback(() => {
    clearStoredSession()
    historyRef.current = []
    setSessionId(null)
    setHistory([])
    setViewIndex(0)
    setError(null)
  }, [setSessionId, setViewIndex])

  return {
    sessionId,
    history,
    viewIndex,
    current: history[viewIndex] ?? null,
    isAtTail: viewIndex === history.length - 1,
    canGoBack: viewIndex > 0,
    canGoForward: viewIndex < history.length - 1,
    isLoading,
    error,
    startSession,
    goBack,
    goForward,
    submitChoice,
    runSampleAutoFill,
    checkCompletion,
    resetSession,
  }
}
