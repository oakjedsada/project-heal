import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useSession } from '../state/SessionContext'

export function ConsentPage() {
  const navigate = useNavigate()
  const { startSession, runSampleAutoFill, isLoading, error } = useSession()
  const [busyAction, setBusyAction] = useState<'start' | 'sample' | null>(null)

  const handleStart = async () => {
    setBusyAction('start')
    try {
      await startSession()
      navigate('/assessment')
    } catch {
      // error is surfaced via the hook's `error` state
    } finally {
      setBusyAction(null)
    }
  }

  const handleSample = async () => {
    setBusyAction('sample')
    try {
      await startSession()
      const outcome = await runSampleAutoFill()
      if (outcome.kind === 'finished') {
        navigate(outcome.nextAction === 'emergency' ? '/emergency' : '/result')
      }
    } catch {
      // error is surfaced via the hook's `error` state
    } finally {
      setBusyAction(null)
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">MindCheck</h1>

      <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm leading-relaxed text-slate-700 shadow-sm">
        <p className="mb-3">
          MindCheck เป็นแบบคัดกรองความเครียดและความเสี่ยงซึมเศร้าเบื้องต้น{' '}
          <strong>สร้างขึ้นเพื่อสาธิตการออกแบบระบบเท่านั้น</strong> ไม่ใช่เครื่องมือวินิจฉัยทางการแพทย์
          และไม่สามารถใช้แทนคำแนะนำจากผู้เชี่ยวชาญได้
        </p>
        <p>
          คำตอบของคุณจะถูกบันทึกไว้ชั่วคราวเพื่อประมวลผลแบบทดสอบเท่านั้น หากพบสัญญาณความเสี่ยงสูง
          ระบบจะแสดงช่องทางขอความช่วยเหลือทันที
        </p>
      </div>

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      <div className="flex flex-col gap-3">
        <button
          type="button"
          onClick={handleStart}
          disabled={isLoading}
          className="min-h-11 rounded-md bg-blue-700 px-4 py-3 font-medium text-white transition-colors hover:bg-blue-800 disabled:opacity-60"
        >
          {busyAction === 'start' ? 'กำลังเริ่ม...' : 'ยินยอมและเริ่มทำแบบประเมิน'}
        </button>

        <button
          type="button"
          onClick={handleSample}
          disabled={isLoading}
          className="min-h-11 rounded-md border border-slate-300 bg-white px-4 py-3 font-medium text-slate-800 transition-colors hover:bg-slate-50 disabled:opacity-60"
        >
          {busyAction === 'sample' ? 'กำลังจำลองคำตอบ...' : 'ลองด้วยข้อมูลตัวอย่าง'}
        </button>
      </div>
    </div>
  )
}
