import type { QuestionDto } from '../api/types'

interface QuestionCardProps {
  question: QuestionDto
  selectedChoiceId: number | null
  disabled: boolean
  onSelect: (choiceId: number) => void
}

export function QuestionCard({ question, selectedChoiceId, disabled, onSelect }: QuestionCardProps) {
  const choices = [...(question.choices ?? [])].sort((a, b) => (a.orderNo ?? 0) - (b.orderNo ?? 0))

  return (
    <fieldset className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm" disabled={disabled}>
      <legend className="mb-4 text-lg font-medium text-slate-900">{question.text}</legend>
      <div className="flex flex-col gap-2">
        {choices.map((choice) => {
          const id = `choice-${choice.choiceId}`
          const isSelected = choice.choiceId === selectedChoiceId
          return (
            <label
              key={choice.choiceId}
              htmlFor={id}
              className={`flex min-h-11 cursor-pointer items-center gap-3 rounded-md border px-4 py-3 text-base transition-colors ${
                isSelected
                  ? 'border-blue-700 bg-blue-50 text-blue-900'
                  : 'border-slate-300 bg-white text-slate-800 hover:bg-slate-50'
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
                className="h-5 w-5 accent-blue-700"
              />
              <span>{choice.label}</span>
            </label>
          )
        })}
      </div>
    </fieldset>
  )
}
