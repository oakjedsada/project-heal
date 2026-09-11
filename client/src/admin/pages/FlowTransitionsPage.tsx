import { useCallback, useEffect, useState } from 'react'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'
import { FlowDiagram } from '../components/FlowDiagram'

type FlowTransitionDto = components['schemas']['FlowTransitionDto']
type InstrumentSummaryDto = components['schemas']['InstrumentSummaryDto']
type InstrumentDetailDto = components['schemas']['InstrumentDetailDto']

const CONDITION_TYPES = ['Always', 'ScoreLevelEquals', 'QuestionScoreAtLeast']

export function FlowTransitionsPage() {
  const { authHeader } = useAuth()
  const { t } = useLanguage()

  const [instruments, setInstruments] = useState<InstrumentSummaryDto[]>([])
  const [transitions, setTransitions] = useState<FlowTransitionDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const [fromInstrumentId, setFromInstrumentId] = useState<string>('')
  const [conditionType, setConditionType] = useState<string>('Always')
  const [conditionValue, setConditionValue] = useState<string>('')
  const [questionId, setQuestionId] = useState<string>('')
  const [toInstrumentId, setToInstrumentId] = useState<string>('')
  const [fromInstrumentDetail, setFromInstrumentDetail] = useState<InstrumentDetailDto | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const loadAll = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    const [instrumentsRes, transitionsRes] = await Promise.all([
      apiClient.GET('/api/admin/instruments', { headers: authHeader }),
      apiClient.GET('/api/admin/flow-transitions', { headers: authHeader }),
    ])
    if (instrumentsRes.error || transitionsRes.error) {
      setError(t('admin.flow.loadFailed'))
    } else {
      setInstruments(instrumentsRes.data ?? [])
      setTransitions(transitionsRes.data ?? [])
    }
    setIsLoading(false)
  }, [authHeader, t])

  useEffect(() => {
    void loadAll()
  }, [loadAll])

  useEffect(() => {
    if (conditionType !== 'QuestionScoreAtLeast' || !fromInstrumentId) {
      setFromInstrumentDetail(null)
      return
    }
    apiClient
      .GET('/api/admin/instruments/{id}', {
        params: { path: { id: Number(fromInstrumentId) } },
        headers: authHeader,
      })
      .then(({ data }) => setFromInstrumentDetail(data ?? null))
  }, [conditionType, fromInstrumentId, authHeader])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setFormError(null)

    if (!toInstrumentId) {
      setFormError(t('admin.flow.selectDestination'))
      return
    }

    const { error: apiError, response } = await apiClient.POST('/api/admin/flow-transitions', {
      headers: authHeader,
      body: {
        fromInstrumentId: fromInstrumentId ? Number(fromInstrumentId) : null,
        conditionType,
        questionId: conditionType === 'QuestionScoreAtLeast' && questionId ? Number(questionId) : null,
        conditionValue: conditionType === 'Always' ? null : conditionValue,
        toInstrumentId: Number(toInstrumentId),
      },
    })

    if (apiError || !response.ok) {
      setFormError(t('admin.flow.createFailed', { status: response.status }))
      return
    }

    setConditionValue('')
    setQuestionId('')
    setToInstrumentId('')
    await loadAll()
  }

  const handleDelete = async (id: number) => {
    await apiClient.DELETE('/api/admin/flow-transitions/{id}', {
      params: { path: { id } },
      headers: authHeader,
    })
    await loadAll()
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <AdminHeader title={t('admin.flow.title')} />

      {error && <p className="mb-4 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      {!isLoading && (
        <div className="mb-6 overflow-x-auto">
          <FlowDiagram instruments={instruments} transitions={transitions} />
        </div>
      )}

      <section className="mb-6 rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">{t('admin.flow.addNew')}</h2>
        <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-3">
          <label className="flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.flow.from')}
            <select
              value={fromInstrumentId}
              onChange={(e) => setFromInstrumentId(e.target.value)}
              className={selectClass}
            >
              <option value="">{t('admin.flow.sessionStart')}</option>
              {instruments.map((i) => (
                <option key={i.instrumentId} value={i.instrumentId}>
                  {i.code} — {i.name}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.flow.condition')}
            <select value={conditionType} onChange={(e) => setConditionType(e.target.value)} className={selectClass}>
              {CONDITION_TYPES.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </label>

          {conditionType === 'ScoreLevelEquals' && (
            <label className="col-span-2 flex flex-col gap-1 text-sm text-stone-700">
              {t('admin.flow.levelMatch')}
              <input
                required
                value={conditionValue}
                onChange={(e) => setConditionValue(e.target.value)}
                className={selectClass}
                placeholder='เช่น "Positive"'
              />
            </label>
          )}

          {conditionType === 'QuestionScoreAtLeast' && (
            <>
              <label className="flex flex-col gap-1 text-sm text-stone-700">
                {t('admin.flow.question')}
                <select value={questionId} onChange={(e) => setQuestionId(e.target.value)} className={selectClass}>
                  <option value="">{t('admin.flow.selectQuestion')}</option>
                  {fromInstrumentDetail?.questions?.map((q) => (
                    <option key={q.questionId} value={q.questionId}>
                      {t('admin.flow.questionItem', { order: q.orderNo!, text: q.text! })}
                    </option>
                  ))}
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm text-stone-700">
                {t('admin.flow.threshold')}
                <input
                  required
                  type="number"
                  value={conditionValue}
                  onChange={(e) => setConditionValue(e.target.value)}
                  className={selectClass}
                />
              </label>
            </>
          )}

          <label className="col-span-2 flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.flow.to')}
            <select value={toInstrumentId} onChange={(e) => setToInstrumentId(e.target.value)} className={selectClass}>
              <option value="">{t('admin.flow.selectInstrument')}</option>
              {instruments.map((i) => (
                <option key={i.instrumentId} value={i.instrumentId}>
                  {i.code} — {i.name}
                </option>
              ))}
            </select>
          </label>

          {formError && <p className="col-span-2 text-sm text-red-700">{formError}</p>}

          <button
            type="submit"
            className="col-span-2 min-h-11 rounded-full bg-pink-500 px-4 py-2 font-medium text-white transition-colors hover:bg-pink-600"
          >
            {t('admin.flow.submit')}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">{t('admin.flow.allTransitions')}</h2>
        <ul className="flex flex-col gap-2 text-sm">
          {transitions.map((tr) => (
            <li key={tr.id} className="flex items-center justify-between rounded-xl border border-stone-100 px-3 py-2">
              <span className="text-stone-700">
                {tr.fromInstrumentCode ?? t('admin.flow.sessionStartLabel')} → {tr.toInstrumentCode} ({tr.conditionType}
                {tr.conditionValue ? `: ${tr.conditionValue}` : ''})
              </span>
              <button
                type="button"
                onClick={() => handleDelete(tr.id!)}
                className="rounded-lg border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50"
              >
                {t('admin.flow.remove')}
              </button>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}

const selectClass = 'min-h-11 rounded-lg border border-stone-300 px-2 py-1 text-sm focus:border-pink-400'
