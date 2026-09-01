import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAdminAuth } from '../AdminAuthContext'
import { FlowDiagram } from '../components/FlowDiagram'

type FlowTransitionDto = components['schemas']['FlowTransitionDto']
type InstrumentSummaryDto = components['schemas']['InstrumentSummaryDto']
type InstrumentDetailDto = components['schemas']['InstrumentDetailDto']

const CONDITION_TYPES = ['Always', 'ScoreLevelEquals', 'QuestionScoreAtLeast']

export function FlowTransitionsPage() {
  const { authHeader } = useAdminAuth()

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
      setError('โหลดข้อมูลไม่สำเร็จ')
    } else {
      setInstruments(instrumentsRes.data ?? [])
      setTransitions(transitionsRes.data ?? [])
    }
    setIsLoading(false)
  }, [authHeader])

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
      setFormError('กรุณาเลือกแบบประเมินปลายทาง')
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
      setFormError(`สร้างเส้นทางไม่สำเร็จ (${response.status})`)
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
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-semibold text-slate-900">เส้นทางแบบประเมิน (flow transitions)</h1>
        <Link to="/admin/instruments/new" className="text-sm text-blue-700 underline">
          สร้างแบบประเมินใหม่
        </Link>
      </div>

      {error && <p className="mb-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      {!isLoading && (
        <div className="mb-6 overflow-x-auto">
          <FlowDiagram instruments={instruments} transitions={transitions} />
        </div>
      )}

      <section className="mb-6 rounded-lg border border-slate-200 bg-white p-4">
        <h2 className="mb-3 font-medium text-slate-900">เพิ่มเส้นทางใหม่</h2>
        <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            จาก (จบแบบประเมินไหน)
            <select
              value={fromInstrumentId}
              onChange={(e) => setFromInstrumentId(e.target.value)}
              className={selectClass}
            >
              <option value="">เริ่มต้น session (ไม่มี from)</option>
              {instruments.map((i) => (
                <option key={i.instrumentId} value={i.instrumentId}>
                  {i.code} — {i.name}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            เงื่อนไข
            <select value={conditionType} onChange={(e) => setConditionType(e.target.value)} className={selectClass}>
              {CONDITION_TYPES.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </label>

          {conditionType === 'ScoreLevelEquals' && (
            <label className="col-span-2 flex flex-col gap-1 text-sm text-slate-700">
              ระดับ (level) ที่ต้องตรงกัน
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
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                คำถาม (ของแบบประเมินต้นทาง)
                <select value={questionId} onChange={(e) => setQuestionId(e.target.value)} className={selectClass}>
                  <option value="">เลือกคำถาม</option>
                  {fromInstrumentDetail?.questions?.map((q) => (
                    <option key={q.questionId} value={q.questionId}>
                      ข้อ {q.orderNo}: {q.text}
                    </option>
                  ))}
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm text-slate-700">
                threshold (คะแนนขั้นต่ำ)
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

          <label className="col-span-2 flex flex-col gap-1 text-sm text-slate-700">
            ไป (แบบประเมินปลายทาง)
            <select value={toInstrumentId} onChange={(e) => setToInstrumentId(e.target.value)} className={selectClass}>
              <option value="">เลือกแบบประเมิน</option>
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
            className="col-span-2 min-h-11 rounded-md bg-blue-700 px-4 py-2 font-medium text-white hover:bg-blue-800"
          >
            เพิ่มเส้นทาง
          </button>
        </form>
      </section>

      <section className="rounded-lg border border-slate-200 bg-white p-4">
        <h2 className="mb-3 font-medium text-slate-900">เส้นทางทั้งหมด</h2>
        <ul className="flex flex-col gap-2 text-sm">
          {transitions.map((t) => (
            <li key={t.id} className="flex items-center justify-between rounded-md border border-slate-100 px-3 py-2">
              <span>
                {t.fromInstrumentCode ?? 'เริ่มต้น session'} → {t.fromInstrumentCode ? '' : ''}
                {t.toInstrumentCode} ({t.conditionType}
                {t.conditionValue ? `: ${t.conditionValue}` : ''})
              </span>
              <button
                type="button"
                onClick={() => handleDelete(t.id!)}
                className="rounded-md border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50"
              >
                ลบ
              </button>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}

const selectClass = 'min-h-11 rounded-md border border-slate-300 px-2 py-1 text-sm'
