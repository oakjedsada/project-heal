import { useEffect, useState } from 'react'
import { apiClient } from '../api/client'
import type { ResultResponse } from '../api/types'
import { useAuth } from './AuthContext'
import { loadSessionId } from './localPersistence'

export function useSessionResult() {
  const { authHeader } = useAuth()
  const [result, setResult] = useState<ResultResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const sessionId = loadSessionId()

  useEffect(() => {
    if (!sessionId) {
      setIsLoading(false)
      return
    }

    let cancelled = false
    apiClient
      .GET('/api/sessions/{id}/result', { params: { path: { id: sessionId } }, headers: authHeader })
      .then(({ data, error: apiError }) => {
        if (cancelled) return
        if (apiError || !data) {
          setError('โหลดผลลัพธ์ไม่สำเร็จ')
          return
        }
        setResult(data)
      })
      .catch(() => {
        if (!cancelled) setError('โหลดผลลัพธ์ไม่สำเร็จ')
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [sessionId, authHeader])

  return { result, isLoading, error, sessionId }
}
