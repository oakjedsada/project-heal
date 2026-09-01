import { Navigate, useNavigate } from 'react-router-dom'
import { clearStoredSession } from '../state/localPersistence'
import { useSessionResult } from '../state/useSessionResult'

export function EmergencyPage() {
  const navigate = useNavigate()
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
      <div
        role="alert"
        className="rounded-lg border-2 border-red-800 bg-red-700 p-5 text-white shadow-md"
      >
        <h1 className="text-xl font-bold">พบสัญญาณความเสี่ยงที่ควรได้รับความช่วยเหลือทันที</h1>
        <p className="mt-2 text-sm leading-relaxed">
          ผลการประเมินบ่งชี้ว่าคุณอาจกำลังเผชิญความเสี่ยงที่ต้องการความช่วยเหลือจากผู้เชี่ยวชาญโดยเร็ว
          กรุณาติดต่อช่องทางด้านล่างนี้
        </p>

        {isLoading && <p className="mt-4 text-sm">กำลังโหลดช่องทางช่วยเหลือ...</p>}

        <ul className="mt-4 flex flex-col gap-2">
          {result?.helpResources?.map((resource, index) => (
            <li key={index}>
              <a
                href={`tel:${resource.contact}`}
                className="flex min-h-11 items-center justify-between rounded-md bg-white px-4 py-3 font-semibold text-red-800 hover:bg-red-50"
              >
                <span>{resource.label}</span>
                <span>{resource.contact}</span>
              </a>
            </li>
          ))}
        </ul>
      </div>

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}

      {result?.results && result.results.length > 0 && (
        <details className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-700">
          <summary className="cursor-pointer font-medium text-slate-900">ดูรายละเอียดผลการประเมิน</summary>
          <div className="mt-3 flex flex-col gap-3">
            {result.results.map((instrumentResult, index) => (
              <div key={`${instrumentResult.instrumentCode}-${index}`}>
                <p className="text-xs font-medium uppercase tracking-wide text-slate-400">
                  {instrumentResult.instrumentCode}
                </p>
                <p className="font-semibold text-slate-900">{instrumentResult.level}</p>
                <p className="mt-1">{instrumentResult.interpretation}</p>
              </div>
            ))}
          </div>
        </details>
      )}

      <button
        type="button"
        onClick={handleRestart}
        className="min-h-11 rounded-md border border-slate-300 bg-white px-4 py-3 font-medium text-slate-800 hover:bg-slate-50"
      >
        เริ่มทำแบบประเมินใหม่
      </button>
    </div>
  )
}
