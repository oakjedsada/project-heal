import { Navigate, useNavigate } from 'react-router-dom'
import { clearStoredSession } from '../state/localPersistence'
import { useLanguage } from '../state/LanguageContext'
import { useSessionResult } from '../state/useSessionResult'

export function EmergencyPage() {
  const navigate = useNavigate()
  const { t } = useLanguage()
  const { result, isLoading, error, sessionId } = useSessionResult()

  if (!sessionId) {
    return <Navigate to="/" replace />
  }

  const handleRestart = () => {
    clearStoredSession()
    navigate('/')
  }

  return (
    <div className="flex flex-col gap-4">
      <div role="alert" className="rounded-2xl border-2 border-red-800 bg-red-700 p-5 text-white shadow-lg shadow-red-900/20">
        <h1 className="text-xl font-bold leading-snug">{t('emergency.title')}</h1>
        <p className="mt-2 text-sm leading-relaxed text-red-50">{t('emergency.description')}</p>

        {isLoading && <p className="mt-4 text-sm">{t('emergency.loadingResources')}</p>}

        <ul className="mt-4 flex flex-col gap-2">
          {result?.helpResources?.map((resource, index) => (
            <li key={index}>
              <a
                href={`tel:${resource.contact}`}
                className="flex min-h-11 items-center justify-between rounded-xl bg-white px-4 py-3 font-semibold text-red-800 transition-colors hover:bg-red-50"
              >
                <span>{resource.label}</span>
                <span>{resource.contact}</span>
              </a>
            </li>
          ))}
        </ul>
      </div>

      {error && (
        <p role="alert" className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      {result?.results && result.results.length > 0 && (
        <details className="rounded-2xl border border-stone-200 bg-white p-4 text-sm text-stone-600 shadow-sm shadow-stone-900/5">
          <summary className="cursor-pointer font-medium text-stone-900">{t('emergency.viewDetails')}</summary>
          <div className="mt-3 flex flex-col gap-3">
            {result.results.map((instrumentResult, index) => (
              <div key={`${instrumentResult.instrumentCode}-${index}`}>
                <p className="text-xs font-semibold uppercase tracking-wide text-stone-400">
                  {instrumentResult.instrumentCode}
                </p>
                <p className="font-semibold text-stone-900">{instrumentResult.level}</p>
                <p className="mt-1">{instrumentResult.interpretation}</p>
              </div>
            ))}
          </div>
        </details>
      )}

      <button
        type="button"
        onClick={handleRestart}
        className="min-h-11 rounded-full border border-stone-300 bg-white px-6 py-3 font-medium text-stone-700 transition-colors hover:bg-stone-50"
      >
        {t('result.restart')}
      </button>
    </div>
  )
}
