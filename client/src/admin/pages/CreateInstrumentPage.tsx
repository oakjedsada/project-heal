import { useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../../api/client'
import { useAdminAuth } from '../AdminAuthContext'

interface ChoiceForm {
  label: string
  score: number
  orderNo: number
}

interface QuestionForm {
  text: string
  orderNo: number
  choices: ChoiceForm[]
}

interface ScoringRuleForm {
  min: number
  max: number
  level: string
  interpretation: string
  advice: string
}

interface RiskRuleForm {
  questionOrderNo: number
  operator: string
  threshold: number
  action: string
}

function emptyChoice(orderNo: number): ChoiceForm {
  return { label: '', score: 0, orderNo }
}

function emptyQuestion(orderNo: number): QuestionForm {
  return { text: '', orderNo, choices: [emptyChoice(1), emptyChoice(2)] }
}

function emptyScoringRule(): ScoringRuleForm {
  return { min: 0, max: 0, level: '', interpretation: '', advice: '' }
}

function emptyRiskRule(): RiskRuleForm {
  return { questionOrderNo: 1, operator: 'GreaterThanOrEqual', threshold: 1, action: 'emergency' }
}

const RISK_OPERATORS = ['GreaterThan', 'GreaterThanOrEqual', 'LessThan', 'LessThanOrEqual', 'Equal', 'NotEqual']

export function CreateInstrumentPage() {
  const { authHeader } = useAdminAuth()

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [version, setVersion] = useState('1.0')
  const [source, setSource] = useState('')
  const [questions, setQuestions] = useState<QuestionForm[]>([emptyQuestion(1)])
  const [scoringRules, setScoringRules] = useState<ScoringRuleForm[]>([emptyScoringRule()])
  const [riskRules, setRiskRules] = useState<RiskRuleForm[]>([])

  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

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

  const addRiskRule = () => setRiskRules((rs) => [...rs, emptyRiskRule()])
  const removeRiskRule = (index: number) => setRiskRules((rs) => rs.filter((_, i) => i !== index))
  const updateRiskRule = (index: number, patch: Partial<RiskRuleForm>) =>
    setRiskRules((rs) => rs.map((r, i) => (i === index ? { ...r, ...patch } : r)))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setSuccessMessage(null)
    setIsSubmitting(true)

    try {
      const { data, error: apiError, response } = await apiClient.POST('/api/admin/instruments', {
        headers: authHeader,
        body: {
          code,
          name,
          version,
          source,
          isActive: true,
          questions,
          scoringRules,
          riskRules,
        },
      })

      if (apiError || !response.ok) {
        const detail = (apiError as { detail?: string } | undefined)?.detail;
        setError(detail ?? `สร้างแบบประเมินไม่สำเร็จ (${response.status})`)
        return
      }

      setSuccessMessage(
        `สร้างแบบประเมิน "${data!.code}" สำเร็จ (instrument id: ${data!.instrumentId}) — ไปหน้า "เส้นทางแบบประเมิน" เพื่อผูกเข้ากับ flow`,
      )
      setCode('')
      setName('')
      setSource('')
      setQuestions([emptyQuestion(1)])
      setScoringRules([emptyScoringRule()])
      setRiskRules([])
    } catch {
      setError('เกิดข้อผิดพลาดที่ไม่คาดคิด')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-semibold text-slate-900">สร้างแบบประเมินใหม่</h1>
        <Link to="/admin/flow-transitions" className="text-sm text-blue-700 underline">
          จัดการเส้นทางแบบประเมิน
        </Link>
      </div>

      {error && (
        <p role="alert" className="mb-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}
      {successMessage && (
        <p role="status" className="mb-4 rounded-md bg-green-50 px-3 py-2 text-sm text-green-800">
          {successMessage}
        </p>
      )}

      <form onSubmit={handleSubmit} className="flex flex-col gap-6">
        <section className="rounded-lg border border-slate-200 bg-white p-4">
          <h2 className="mb-3 font-medium text-slate-900">ข้อมูลพื้นฐาน</h2>
          <div className="grid grid-cols-2 gap-3">
            <Field label="รหัส (code)">
              <input required value={code} onChange={(e) => setCode(e.target.value)} className={inputClass} />
            </Field>
            <Field label="ชื่อ (name)">
              <input required value={name} onChange={(e) => setName(e.target.value)} className={inputClass} />
            </Field>
            <Field label="เวอร์ชัน">
              <input required value={version} onChange={(e) => setVersion(e.target.value)} className={inputClass} />
            </Field>
            <Field label="แหล่งที่มา (source)">
              <input required value={source} onChange={(e) => setSource(e.target.value)} className={inputClass} />
            </Field>
          </div>
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="font-medium text-slate-900">คำถาม</h2>
            <button type="button" onClick={addQuestion} className={addButtonClass}>
              + เพิ่มคำถาม
            </button>
          </div>
          <div className="flex flex-col gap-4">
            {questions.map((q, qIndex) => (
              <div key={qIndex} className="rounded-md border border-slate-200 p-3">
                <div className="mb-2 flex items-center gap-2">
                  <Field label="ลำดับ" className="w-20">
                    <input
                      type="number"
                      value={q.orderNo}
                      onChange={(e) => updateQuestion(qIndex, { orderNo: Number(e.target.value) })}
                      className={inputClass}
                    />
                  </Field>
                  <Field label="ข้อความคำถาม" className="flex-1">
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
                      className={removeButtonClass}
                    >
                      ลบคำถาม
                    </button>
                  )}
                </div>

                <div className="ml-4 flex flex-col gap-2">
                  {q.choices.map((c, cIndex) => (
                    <div key={cIndex} className="flex items-center gap-2">
                      <input
                        required
                        placeholder="label"
                        value={c.label}
                        onChange={(e) => updateChoice(qIndex, cIndex, { label: e.target.value })}
                        className={`${inputClass} flex-1`}
                      />
                      <input
                        type="number"
                        placeholder="score"
                        value={c.score}
                        onChange={(e) => updateChoice(qIndex, cIndex, { score: Number(e.target.value) })}
                        className={`${inputClass} w-24`}
                      />
                      <input
                        type="number"
                        placeholder="order"
                        value={c.orderNo}
                        onChange={(e) => updateChoice(qIndex, cIndex, { orderNo: Number(e.target.value) })}
                        className={`${inputClass} w-20`}
                      />
                      {q.choices.length > 1 && (
                        <button type="button" onClick={() => removeChoice(qIndex, cIndex)} className={removeButtonClass}>
                          ลบ
                        </button>
                      )}
                    </div>
                  ))}
                  <button type="button" onClick={() => addChoice(qIndex)} className={`${addButtonClass} self-start`}>
                    + เพิ่มตัวเลือก
                  </button>
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="font-medium text-slate-900">เกณฑ์คะแนน (scoring rules)</h2>
            <button type="button" onClick={addScoringRule} className={addButtonClass}>
              + เพิ่มช่วงคะแนน
            </button>
          </div>
          <div className="flex flex-col gap-3">
            {scoringRules.map((r, index) => (
              <div key={index} className="grid grid-cols-6 gap-2 rounded-md border border-slate-200 p-3">
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
                <div className="flex items-center gap-1">
                  <input
                    required
                    placeholder="advice"
                    value={r.advice}
                    onChange={(e) => updateScoringRule(index, { advice: e.target.value })}
                    className={inputClass}
                  />
                  {scoringRules.length > 1 && (
                    <button type="button" onClick={() => removeScoringRule(index)} className={removeButtonClass}>
                      ลบ
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="font-medium text-slate-900">risk rules (ไม่บังคับ)</h2>
            <button type="button" onClick={addRiskRule} className={addButtonClass}>
              + เพิ่ม risk rule
            </button>
          </div>
          <div className="flex flex-col gap-3">
            {riskRules.map((r, index) => (
              <div key={index} className="grid grid-cols-5 gap-2 rounded-md border border-slate-200 p-3">
                <select
                  value={r.questionOrderNo}
                  onChange={(e) => updateRiskRule(index, { questionOrderNo: Number(e.target.value) })}
                  className={inputClass}
                >
                  {questions.map((q) => (
                    <option key={q.orderNo} value={q.orderNo}>
                      คำถามข้อ {q.orderNo}
                    </option>
                  ))}
                </select>
                <select
                  value={r.operator}
                  onChange={(e) => updateRiskRule(index, { operator: e.target.value })}
                  className={inputClass}
                >
                  {RISK_OPERATORS.map((op) => (
                    <option key={op} value={op}>
                      {op}
                    </option>
                  ))}
                </select>
                <input
                  type="number"
                  placeholder="threshold"
                  value={r.threshold}
                  onChange={(e) => updateRiskRule(index, { threshold: Number(e.target.value) })}
                  className={inputClass}
                />
                <input
                  placeholder="action"
                  value={r.action}
                  onChange={(e) => updateRiskRule(index, { action: e.target.value })}
                  className={inputClass}
                />
                <button type="button" onClick={() => removeRiskRule(index)} className={removeButtonClass}>
                  ลบ
                </button>
              </div>
            ))}
          </div>
        </section>

        <button
          type="submit"
          disabled={isSubmitting}
          className="min-h-11 rounded-md bg-blue-700 px-4 py-3 font-medium text-white hover:bg-blue-800 disabled:opacity-60"
        >
          {isSubmitting ? 'กำลังบันทึก...' : 'สร้างแบบประเมิน'}
        </button>
      </form>
    </div>
  )
}

const inputClass = 'min-h-11 rounded-md border border-slate-300 px-2 py-1 text-sm'
const addButtonClass = 'rounded-md border border-blue-300 px-3 py-1.5 text-sm text-blue-700 hover:bg-blue-50'
const removeButtonClass = 'rounded-md border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50'

function Field({ label, children, className }: { label: string; children: React.ReactNode; className?: string }) {
  return (
    <label className={`flex flex-col gap-1 text-sm text-slate-700 ${className ?? ''}`}>
      {label}
      {children}
    </label>
  )
}
