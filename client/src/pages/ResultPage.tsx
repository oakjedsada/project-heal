import { useEffect } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { clearStoredSession } from '../state/localPersistence'
import { useLanguage } from '../state/LanguageContext'
import { useSessionResult } from '../state/useSessionResult'

export function ResultPage() {
  const navigate = useNavigate()
  const { t } = useLanguage()
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
      <h1 className="text-2xl font-semibold tracking-tight text-stone-900">{t('result.title')}</h1>

      {isLoading && <p className="text-stone-500">{t('result.loading')}</p>}

      {error && (
        <p role="alert" className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      {result && !result.isComplete && <p className="text-stone-500">{t('result.notComplete')}</p>}

      {result?.results?.map((instrumentResult, index) => (
        <div
          key={`${instrumentResult.instrumentCode}-${index}`}
          className="rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
        >
          <p className="text-xs font-semibold uppercase tracking-wide text-pink-500">
            {instrumentResult.instrumentCode}
          </p>
          <p className="mt-1 text-lg font-semibold text-stone-900">{instrumentResult.level}</p>
          <p className="mt-2 text-sm leading-relaxed text-stone-600">{instrumentResult.interpretation}</p>
          <p className="mt-2 text-sm leading-relaxed text-stone-500">{instrumentResult.advice}</p>
        </div>
      ))}

      <button
        type="button"
        onClick={handleRestart}
        className="min-h-11 rounded-full border border-stone-300 bg-white px-6 py-3 font-medium text-stone-700 transition-colors hover:border-pink-200 hover:bg-pink-50/50"
      >
        {t('result.restart')}
      </button>
    </div>
  )
}
