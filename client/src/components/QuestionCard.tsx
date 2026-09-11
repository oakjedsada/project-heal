import type { QuestionDto } from '../api/types'
import { CheckIcon } from './icons'

interface QuestionCardProps {
  question: QuestionDto
  selectedChoiceId: number | null
  disabled: boolean
  onSelect: (choiceId: number) => void
}

export function QuestionCard({ question, selectedChoiceId, disabled, onSelect }: QuestionCardProps) {
  const choices = [...(question.choices ?? [])].sort((a, b) => (a.orderNo ?? 0) - (b.orderNo ?? 0))
  const headingId = `question-heading-${question.questionId}`

  return (
    <div
      role="radiogroup"
      aria-labelledby={headingId}
      className="rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
    >
      <p id={headingId} className="mb-4 text-lg font-medium leading-relaxed text-stone-900">
        {question.text}
      </p>
      <div className="flex flex-col gap-2.5">
        {choices.map((choice) => {
          const id = `choice-${choice.choiceId}`
          const isSelected = choice.choiceId === selectedChoiceId
          return (
            <label
              key={choice.choiceId}
              htmlFor={id}
              className={`flex min-h-11 cursor-pointer items-center gap-3 rounded-xl border px-4 py-3 text-base transition-all ${
                isSelected
                  ? 'border-pink-500 bg-pink-50 text-pink-900 ring-1 ring-pink-500'
                  : 'border-stone-200 bg-white text-stone-700 hover:border-pink-200 hover:bg-pink-50/40'
              } ${disabled ? 'cursor-not-allowed opacity-60' : ''}`}
            >
              <input
                id={id}
                type="radio"
                name={`question-${question.questionId}`}
                value={choice.choiceId}
                checked={isSelected}
                disabled={disabled}
                onChange={() => onSelect(choice.choiceId!)}
                className="sr-only"
              />
              <span
                className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 transition-colors ${
                  isSelected ? 'border-pink-500 bg-pink-500 text-white' : 'border-stone-300 text-transparent'
                }`}
              >
                <CheckIcon className="h-3 w-3" />
              </span>
              <span>{choice.label}</span>
            </label>
          )
        })}
      </div>
    </div>
  )
}
