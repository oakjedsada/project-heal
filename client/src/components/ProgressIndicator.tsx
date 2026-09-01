interface ProgressIndicatorProps {
  answeredCount: number
  isComplete: boolean
}

// Total question count per instrument is intentionally unknown to the
// frontend (that structure lives entirely in the backend's seed data), so
// this renders an open-ended "how far so far" indicator rather than a
// misleading "N of M" percentage.
export function ProgressIndicator({ answeredCount, isComplete }: ProgressIndicatorProps) {
  const widthPercent = isComplete ? 100 : Math.min(92, (answeredCount / (answeredCount + 2)) * 100)

  return (
    <div className="mb-4">
      <div className="h-2 w-full overflow-hidden rounded-full bg-slate-200" aria-hidden="true">
        <div
          className="h-2 rounded-full bg-blue-700 transition-all duration-300 ease-out"
          style={{ width: `${widthPercent}%` }}
        />
      </div>
      <p className="mt-1 text-sm text-slate-600">
        {isComplete ? 'ทำแบบประเมินครบแล้ว' : `ข้อที่ ${answeredCount + 1}`}
      </p>
    </div>
  )
}
