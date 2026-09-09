import { useEffect, useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { ProgressIndicator } from '../components/ProgressIndicator'
import { QuestionCard } from '../components/QuestionCard'
import { loadSessionId } from '../state/localPersistence'
import { useSession } from '../state/SessionContext'

export function AssessmentPage() {
  const navigate = useNavigate()
  const {
    sessionId,
    history,
    viewIndex,
    current,
    isAtTail,
    canGoBack,
    canGoForward,
    isLoading,
    error,
    goBack,
    goForward,
    submitChoice,
    checkCompletion,
  } = useSession()

  const [rejectedNotice, setRejectedNotice] = useState(false)
  const [checkedAlreadyFinished, setCheckedAlreadyFinished] = useState(false)

  const hasStoredSession = Boolean(sessionId ?? loadSessionId())

  useEffect(() => {
    if (!hasStoredSession || isLoading || current || checkedAlreadyFinished) return
    setCheckedAlreadyFinished(true)
    void checkCompletion().then((result) => {
      if (!result) return
      navigate(result.nextAction === 'emergency' ? '/emergency' : '/result', { replace: true })
    })
  }, [hasStoredSession, isLoading, current, checkedAlreadyFinished, checkCompletion, navigate])

  if (!hasStoredSession) {
    return <Navigate to="/" replace />
  }

  const answeredCount = history.filter((h) => h.choiceId !== null).length

  const handleSelect = async (choiceId: number) => {
    setRejectedNotice(false)
    const outcome = await submitChoice(choiceId)
    if (outcome.kind === 'finished') {
      navigate(outcome.nextAction === 'emergency' ? '/emergency' : '/result')
    } else if (outcome.kind === 'rejected') {
      setRejectedNotice(true)
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <ProgressIndicator answeredCount={answeredCount} isComplete={false} />

      {error && (
        <p role="alert" className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      {rejectedNotice && (
        <p role="alert" className="rounded-xl bg-amber-50 px-3 py-2 text-sm text-amber-900">
          คำตอบข้อนี้ถูกประมวลผลไปแล้วและไม่สามารถแก้ไขได้อีก
        </p>
      )}

      {current ? (
        <QuestionCard
          question={current.question}
          selectedChoiceId={current.choiceId}
          disabled={isLoading || current.editable === false}
          onSelect={handleSelect}
        />
      ) : (
        <p className="text-stone-500">กำลังโหลดคำถาม...</p>
      )}

      <div className="flex justify-between">
        <button
          type="button"
          onClick={goBack}
          disabled={!canGoBack || isLoading}
          className="min-h-11 rounded-full border border-stone-300 bg-white px-5 py-2 text-stone-700 transition-colors hover:border-pink-200 hover:bg-pink-50/50 disabled:opacity-40"
        >
          ย้อนกลับ
        </button>
        <button
          type="button"
          onClick={goForward}
          disabled={!canGoForward || isLoading}
          className="min-h-11 rounded-full border border-stone-300 bg-white px-5 py-2 text-stone-700 transition-colors hover:border-pink-200 hover:bg-pink-50/50 disabled:opacity-40"
        >
          ถัดไป
        </button>
      </div>

      <p className="text-center text-xs text-stone-400">
        {isAtTail ? 'กำลังทำข้อล่าสุด' : `กำลังดูข้อที่ ${viewIndex + 1} จากทั้งหมด ${history.length} ข้อที่ทำมา`}
      </p>
    </div>
  )
}
