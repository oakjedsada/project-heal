import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'

type InstrumentSummaryDto = components['schemas']['InstrumentSummaryDto']

export function InstrumentManagementPage() {
  const { authHeader } = useAuth()
  const { t } = useLanguage()

  const [instruments, setInstruments] = useState<InstrumentSummaryDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [activatingId, setActivatingId] = useState<number | null>(null)

  const loadInstruments = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    const { data, error: apiError } = await apiClient.GET('/api/admin/instruments', { headers: authHeader })
    if (apiError) {
      setError(t('admin.instruments.loadFailed'))
    } else {
      setInstruments(data ?? [])
    }
    setIsLoading(false)
  }, [authHeader, t])

  useEffect(() => {
    void loadInstruments()
  }, [loadInstruments])

  const handleSetActive = async (instrumentId: number) => {
    setError(null)
    setActivatingId(instrumentId)

    const { error: apiError, response } = await apiClient.PATCH('/api/admin/instruments/{id}/activate', {
      params: { path: { id: instrumentId } },
      headers: authHeader,
    })

    setActivatingId(null)

    if (apiError || !response.ok) {
      setError(t('admin.instruments.setActiveFailed', { status: response.status }))
      return
    }

    await loadInstruments()
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <AdminHeader title={t('admin.instruments.title')} />

      <p className="mb-4 text-sm text-stone-500">{t('admin.instruments.description')}</p>

      {error && <p className="mb-4 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        {isLoading ? (
          <p className="text-sm text-stone-500">{t('common.loading')}</p>
        ) : instruments.length === 0 ? (
          <p className="text-sm text-stone-500">{t('admin.instruments.noInstruments')}</p>
        ) : (
          <ul className="flex flex-col gap-2 text-sm">
            {instruments.map((instrument) => (
              <li
                key={instrument.instrumentId}
                className="flex items-center justify-between gap-3 rounded-xl border border-stone-100 px-3 py-2"
              >
                <div className="flex flex-col">
                  <span className="font-medium text-stone-900">{instrument.name}</span>
                  <span className="text-xs text-stone-500">{instrument.code}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Link
                    to={`/admin/instruments/${instrument.instrumentId}/edit`}
                    className="rounded-lg border border-stone-300 px-3 py-1.5 text-xs font-medium text-stone-700 transition-colors hover:bg-stone-50"
                  >
                    {t('admin.instruments.edit')}
                  </Link>
                  {instrument.isActiveStart ? (
                    <span className="rounded-full bg-pink-500 px-3 py-1.5 text-xs font-medium text-white">
                      {t('admin.instruments.active')}
                    </span>
                  ) : (
                    <button
                      type="button"
                      onClick={() => handleSetActive(instrument.instrumentId!)}
                      disabled={activatingId === instrument.instrumentId}
                      className="rounded-lg border border-pink-300 px-3 py-1.5 text-xs font-medium text-pink-700 transition-colors hover:bg-pink-50 disabled:opacity-60"
                    >
                      {activatingId === instrument.instrumentId
                        ? t('admin.instruments.setActiveBusy')
                        : t('admin.instruments.setActive')}
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
