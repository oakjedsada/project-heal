import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { HeartIcon } from '../components/icons'
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
      <div>
        <span className="mb-3 inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
          <HeartIcon className="h-5 w-5" />
        </span>
        <h1 className="text-2xl font-semibold tracking-tight text-stone-900">
          เช็กความเครียด <br className="sm:hidden" />
          และสุขภาพใจเบื้องต้น
        </h1>
        <p className="mt-2 text-sm text-stone-500">ใช้เวลาไม่กี่นาที คำตอบของคุณผูกกับบัญชีที่เข้าสู่ระบบอยู่</p>
      </div>

      <div className="rounded-2xl border border-pink-100 bg-white p-5 text-sm leading-relaxed text-stone-600 shadow-sm shadow-pink-900/5">
        <p className="mb-3">
          MindCheck เป็นแบบคัดกรองความเครียดและความเสี่ยงซึมเศร้าเบื้องต้น{' '}
          <strong className="text-stone-800">สร้างขึ้นเพื่อสาธิตการออกแบบระบบเท่านั้น</strong>{' '}
          ไม่ใช่เครื่องมือวินิจฉัยทางการแพทย์และไม่สามารถใช้แทนคำแนะนำจากผู้เชี่ยวชาญได้
        </p>
        <p>
          คำตอบของคุณจะถูกบันทึกไว้ชั่วคราวเพื่อประมวลผลแบบทดสอบเท่านั้น หากพบสัญญาณความเสี่ยงสูง
          ระบบจะแสดงช่องทางขอความช่วยเหลือทันที
        </p>
      </div>

      {error && (
        <p role="alert" className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      <div className="flex flex-col gap-3">
        <button
          type="button"
          onClick={handleStart}
          disabled={isLoading}
          className="min-h-11 rounded-full bg-pink-500 px-6 py-3 font-medium text-white shadow-md shadow-pink-900/15 transition-all hover:-translate-y-0.5 hover:bg-pink-600 hover:shadow-lg hover:shadow-pink-900/20 disabled:translate-y-0 disabled:opacity-60 disabled:shadow-none"
        >
          {busyAction === 'start' ? 'กำลังเริ่ม...' : 'ยินยอมและเริ่มทำแบบประเมิน'}
        </button>

        <button
          type="button"
          onClick={handleSample}
          disabled={isLoading}
          className="min-h-11 rounded-full border border-stone-300 bg-white px-6 py-3 font-medium text-stone-700 transition-colors hover:border-pink-200 hover:bg-pink-50/50 disabled:opacity-60"
        >
          {busyAction === 'sample' ? 'กำลังจำลองคำตอบ...' : 'ลองด้วยข้อมูลตัวอย่าง'}
        </button>
      </div>
    </div>
  )
}
