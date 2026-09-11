import { useLanguage } from '../state/LanguageContext'

interface ProgressIndicatorProps {
  answeredCount: number
  isComplete: boolean
}

// Total question count per instrument is intentionally unknown to the
// frontend (that structure lives entirely in the backend's seed data), so
// this renders an open-ended "how far so far" indicator rather than a
// misleading "N of M" percentage.
export function ProgressIndicator({ answeredCount, isComplete }: ProgressIndicatorProps) {
  const { t } = useLanguage()
  const widthPercent = isComplete ? 100 : Math.min(92, (answeredCount / (answeredCount + 2)) * 100)

  return (
    <div className="mb-4">
      <div className="flex items-center gap-3">
        <div className="h-2.5 flex-1 overflow-hidden rounded-full bg-pink-100" aria-hidden="true">
          <div
            className="h-2.5 rounded-full bg-gradient-to-r from-pink-400 to-pink-600 transition-all duration-300 ease-out"
            style={{ width: `${widthPercent}%` }}
          />
        </div>
        <span className="flex h-7 min-w-7 shrink-0 items-center justify-center rounded-full bg-pink-100 px-2 text-xs font-semibold text-pink-700">
          {isComplete ? '✓' : answeredCount + 1}
        </span>
      </div>
      <p className="mt-1.5 text-sm text-stone-500">
        {isComplete ? t('progress.complete') : t('progress.item', { n: answeredCount + 1 })}
      </p>
    </div>
  )
}
