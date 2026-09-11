import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '../../api/client'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'

interface ChoiceForm {
  choiceId: number | null
  label: string
  score: number
  orderNo: number
}

interface QuestionForm {
  questionId: number | null
  text: string
  orderNo: number
  choices: ChoiceForm[]
}

interface ScoringRuleForm {
  scoringRuleId: number | null
  min: number
  max: number
  level: string
  interpretation: string
  advice: string
}

function emptyChoice(orderNo: number): ChoiceForm {
  return { choiceId: null, label: '', score: 0, orderNo }
}

function emptyQuestion(orderNo: number): QuestionForm {
  return { questionId: null, text: '', orderNo, choices: [emptyChoice(1), emptyChoice(2)] }
}

function emptyScoringRule(): ScoringRuleForm {
  return { scoringRuleId: null, min: 0, max: 0, level: '', interpretation: '', advice: '' }
}

export function EditInstrumentPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { authHeader } = useAuth()
  const { t } = useLanguage()

  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [version, setVersion] = useState('')
  const [source, setSource] = useState('')
  const [questions, setQuestions] = useState<QuestionForm[]>([])
  const [scoringRules, setScoringRules] = useState<ScoringRuleForm[]>([])

  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    setIsLoading(true)
    setLoadError(null)
    apiClient
      .GET('/api/admin/instruments/{id}/full', { params: { path: { id: Number(id) } }, headers: authHeader })
      .then(({ data, error: apiError }) => {
        if (apiError || !data) {
          setLoadError(t('admin.editInstrument.loadFailed'))
          return
        }
        setCode(data.code ?? '')
        setName(data.name ?? '')
        setVersion(data.version ?? '')
        setSource(data.source ?? '')
        setQuestions(
          (data.questions ?? []).map((q) => ({
            questionId: q.questionId ?? null,
            text: q.text ?? '',
            orderNo: q.orderNo ?? 1,
            choices: (q.choices ?? []).map((c) => ({
              choiceId: c.choiceId ?? null,
              label: c.label ?? '',
              score: c.score ?? 0,
              orderNo: c.orderNo ?? 1,
            })),
          })),
        )
        setScoringRules(
          (data.scoringRules ?? []).map((r) => ({
            scoringRuleId: r.scoringRuleId ?? null,
            min: r.min ?? 0,
            max: r.max ?? 0,
            level: r.level ?? '',
            interpretation: r.interpretation ?? '',
            advice: r.advice ?? '',
          })),
        )
      })
      .finally(() => setIsLoading(false))
  }, [id, authHeader, t])

  const addQuestion = () => setQuestions((qs) => [...qs, emptyQuestion(qs.length + 1)])
  const removeQuestion = (index: number) => setQuestions((qs) => qs.filter((_, i) => i !== index))
  const updateQuestion = (index: number, patch: Partial<QuestionForm>) =>
    setQuestions((qs) => qs.map((q, i) => (i === index ? { ...q, ...patch } : q)))

  const addChoice = (qIndex: number) =>
    setQuestions((qs) =>
      qs.map((q, i) => (i === qIndex ? { ...q, choices: [...q.choices, emptyChoice(q.choices.length + 1)] } : q)),
    )
  const removeChoice = (qIndex: number, cIndex: number) =>
    setQuestions((qs) =>
      qs.map((q, i) => (i === qIndex ? { ...q, choices: q.choices.filter((_, j) => j !== cIndex) } : q)),
    )
  const updateChoice = (qIndex: number, cIndex: number, patch: Partial<ChoiceForm>) =>
    setQuestions((qs) =>
      qs.map((q, i) =>
        i === qIndex
          ? { ...q, choices: q.choices.map((c, j) => (j === cIndex ? { ...c, ...patch } : c)) }
          : q,
      ),
    )

  const addScoringRule = () => setScoringRules((rs) => [...rs, emptyScoringRule()])
  const removeScoringRule = (index: number) => setScoringRules((rs) => rs.filter((_, i) => i !== index))
  const updateScoringRule = (index: number, patch: Partial<ScoringRuleForm>) =>
    setScoringRules((rs) => rs.map((r, i) => (i === index ? { ...r, ...patch } : r)))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return
    setError(null)
    setSuccessMessage(null)
    setIsSubmitting(true)

    const { error: apiError, response } = await apiClient.PUT('/api/admin/instruments/{id}', {
      params: { path: { id: Number(id) } },
      headers: authHeader,
      body: {
        code,
        name,
        version,
        source,
        isActive: true,
        questions: questions.map((q) => ({
          questionId: q.questionId,
          text: q.text,
          orderNo: q.orderNo,
          choices: q.choices.map((c) => ({ choiceId: c.choiceId, label: c.label, score: c.score, orderNo: c.orderNo })),
        })),
        scoringRules: scoringRules.map((r) => ({
          scoringRuleId: r.scoringRuleId,
          min: r.min,
          max: r.max,
          level: r.level,
          interpretation: r.interpretation,
          advice: r.advice,
        })),
      },
    })

    setIsSubmitting(false)

    if (apiError || !response.ok) {
      const detail = (apiError as { detail?: string } | undefined)?.detail
      setError(detail ?? t('admin.editInstrument.saveFailed', { status: response.status }))
      return
    }

    navigate('/admin/instruments')
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <AdminHeader title={t('admin.editInstrument.title')} />

      {isLoading && <p className="text-stone-500">{t('common.loading')}</p>}
      {loadError && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{loadError}</p>}

      {!isLoading && !loadError && (
        <>
          {error && (
            <p role="alert" className="mb-4 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">
              {error}
            </p>
          )}
          {successMessage && (
            <p role="status" className="mb-4 rounded-xl bg-pink-50 px-3 py-2 text-sm text-pink-800">
              {successMessage}
            </p>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-6">
            <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
              <h2 className="mb-3 font-medium text-stone-900">{t('admin.createInstrument.basicInfo')}</h2>
              <div className="grid grid-cols-2 gap-3">
                <Field label={t('admin.createInstrument.code')}>
                  <input required value={code} onChange={(e) => setCode(e.target.value)} className={inputClass} />
                </Field>
                <Field label={t('admin.createInstrument.name')}>
                  <input required value={name} onChange={(e) => setName(e.target.value)} className={inputClass} />
                </Field>
                <Field label={t('admin.createInstrument.version')}>
                  <input required value={version} onChange={(e) => setVersion(e.target.value)} className={inputClass} />
                </Field>
                <Field label={t('admin.createInstrument.source')}>
                  <input required value={source} onChange={(e) => setSource(e.target.value)} className={inputClass} />
                </Field>
              </div>
            </section>

            <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
              <div className="mb-3 flex items-center justify-between">
                <h2 className="font-medium text-stone-900">{t('admin.createInstrument.questions')}</h2>
                <button type="button" onClick={addQuestion} className={addButtonClass}>
                  {t('admin.createInstrument.addQuestion')}
                </button>
              </div>
              <div className="flex flex-col gap-4">
                {questions.map((q, qIndex) => (
                  <div key={q.questionId ?? `new-${qIndex}`} className="rounded-xl border border-pink-100 bg-pink-50/30 p-3">
                    <div className="mb-2 flex items-center gap-2">
                      <Field label={t('admin.createInstrument.order')} className="w-20">
                        <input
                          type="number"
                          value={q.orderNo}
                          onChange={(e) => updateQuestion(qIndex, { orderNo: Number(e.target.value) })}
                          className={inputClass}
                        />
                      </Field>
                      <Field label={t('admin.createInstrument.questionText')} className="min-w-0 flex-1">
                        <input
                          required
                          value={q.text}
                          onChange={(e) => updateQuestion(qIndex, { text: e.target.value })}
                          className={inputClass}
                        />
                      </Field>
                      {questions.length > 1 && (
                        <button
                          type="button"
                          onClick={() => removeQuestion(qIndex)}
                          className={`${removeButtonClass} shrink-0`}
                        >
                          {t('admin.createInstrument.removeQuestion')}
                        </button>
                      )}
                    </div>

                    <div className="ml-4 flex flex-col gap-2">
                      {q.choices.map((c, cIndex) => (
                        <div key={c.choiceId ?? `new-${cIndex}`} className="flex items-center gap-2">
                          <input
                            required
                            placeholder="label"
                            value={c.label}
                            onChange={(e) => updateChoice(qIndex, cIndex, { label: e.target.value })}
                            className={`${inputClass} min-w-0 flex-1`}
                          />
                          <input
                            type="number"
                            placeholder="score"
                            value={c.score}
                            onChange={(e) => updateChoice(qIndex, cIndex, { score: Number(e.target.value) })}
                            className={`${inputClass} w-24 shrink-0`}
                          />
                          <input
                            type="number"
                            placeholder="order"
                            value={c.orderNo}
                            onChange={(e) => updateChoice(qIndex, cIndex, { orderNo: Number(e.target.value) })}
                            className={`${inputClass} w-20 shrink-0`}
                          />
                          {q.choices.length > 1 && (
                            <button
                              type="button"
                              onClick={() => removeChoice(qIndex, cIndex)}
                              className={`${removeButtonClass} shrink-0`}
                            >
                              {t('admin.createInstrument.remove')}
                            </button>
                          )}
                        </div>
                      ))}
                      <button type="button" onClick={() => addChoice(qIndex)} className={`${addButtonClass} self-start`}>
                        {t('admin.createInstrument.addChoice')}
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </section>

            <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
              <div className="mb-3 flex items-center justify-between">
                <h2 className="font-medium text-stone-900">{t('admin.createInstrument.scoringRules')}</h2>
                <button type="button" onClick={addScoringRule} className={addButtonClass}>
                  {t('admin.createInstrument.addScoringRule')}
                </button>
              </div>
              <div className="flex flex-col gap-3">
                {scoringRules.map((r, index) => (
                  <div
                    key={r.scoringRuleId ?? `new-${index}`}
                    className="grid grid-cols-6 gap-2 rounded-xl border border-pink-100 bg-pink-50/30 p-3"
                  >
                    <input
                      type="number"
                      placeholder="min"
                      value={r.min}
                      onChange={(e) => updateScoringRule(index, { min: Number(e.target.value) })}
                      className={inputClass}
                    />
                    <input
                      type="number"
                      placeholder="max"
                      value={r.max}
                      onChange={(e) => updateScoringRule(index, { max: Number(e.target.value) })}
                      className={inputClass}
                    />
                    <input
                      required
                      placeholder="level"
                      value={r.level}
                      onChange={(e) => updateScoringRule(index, { level: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      required
                      placeholder="interpretation"
                      value={r.interpretation}
                      onChange={(e) => updateScoringRule(index, { interpretation: e.target.value })}
                      className={`${inputClass} col-span-2`}
                    />
                    <div className="flex min-w-0 items-center gap-1">
                      <input
                        required
                        placeholder="advice"
                        value={r.advice}
                        onChange={(e) => updateScoringRule(index, { advice: e.target.value })}
                        className={`${inputClass} min-w-0 flex-1`}
                      />
                      {scoringRules.length > 1 && (
                        <button
                          type="button"
                          onClick={() => removeScoringRule(index)}
                          className={`${removeButtonClass} shrink-0`}
                        >
                          {t('admin.createInstrument.remove')}
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </section>

            <div className="flex gap-3">
              <button
                type="submit"
                disabled={isSubmitting}
                className="min-h-11 flex-1 rounded-full bg-pink-500 px-4 py-3 font-medium text-white transition-colors hover:bg-pink-600 disabled:opacity-60"
              >
                {isSubmitting ? t('admin.editInstrument.saveBusy') : t('admin.editInstrument.save')}
              </button>
              <button
                type="button"
                onClick={() => navigate('/admin/instruments')}
                className="min-h-11 rounded-full border border-stone-300 bg-white px-4 py-3 font-medium text-stone-700 transition-colors hover:bg-stone-50"
              >
                {t('admin.editInstrument.cancel')}
              </button>
            </div>
          </form>
        </>
      )}
    </div>
  )
}

const inputClass = 'min-h-11 rounded-lg border border-stone-300 px-2 py-1 text-sm focus:border-pink-400'
const addButtonClass = 'rounded-lg border border-pink-300 px-3 py-1.5 text-sm text-pink-700 hover:bg-pink-50'
const removeButtonClass = 'rounded-lg border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50'

function Field({ label, children, className }: { label: string; children: React.ReactNode; className?: string }) {
  return (
    <label className={`flex flex-col gap-1 text-sm text-stone-700 ${className ?? ''}`}>
      {label}
      {children}
    </label>
  )
}
