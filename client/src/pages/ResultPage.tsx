import { useEffect } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { clearStoredSession } from '../state/localPersistence'
import { useSessionResult } from '../state/useSessionResult'

export function ResultPage() {
  const navigate = useNavigate()
  const { result, isLoading, error, sessionId } = useSessionResult()

  useEffect(() => {
    if (result?.nextAction === 'emergency') {
      navigate('/emergency', { replace: true })
    }
  }, [result, navigate])

  if (!sessionId) {
    return <Navigate to="/" replace />
  }

  const handleRestart = () => {
    clearStoredSession()
    navigate('/')
  }

  return (
    <div className="flex flex-col gap-4">
      <h1 className="text-2xl font-semibold text-slate-900">ผลการประเมิน</h1>

      {isLoading && <p className="text-slate-600">กำลังโหลดผลลัพธ์...</p>}

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      {result && !result.isComplete && <p className="text-slate-600">ยังทำแบบประเมินไม่เสร็จ</p>}

      {result?.results?.map((instrumentResult, index) => (
        <div
          key={`${instrumentResult.instrumentCode}-${index}`}
          className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm"
        >
          <p className="text-xs font-medium uppercase tracking-wide text-slate-400">
            {instrumentResult.instrumentCode}
          </p>
          <p className="mt-1 text-lg font-semibold text-slate-900">{instrumentResult.level}</p>
          <p className="mt-2 text-sm text-slate-700">{instrumentResult.interpretation}</p>
          <p className="mt-2 text-sm text-slate-600">{instrumentResult.advice}</p>
        </div>
      ))}

      <button
        type="button"
        onClick={handleRestart}
        className="min-h-11 rounded-md border border-slate-300 bg-white px-4 py-3 font-medium text-slate-800 hover:bg-slate-50"
      >
        เริ่มทำแบบประเมินใหม่
      </button>
    </div>
  )
}
