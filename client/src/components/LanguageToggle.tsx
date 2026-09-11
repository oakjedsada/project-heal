import { useLanguage } from '../state/LanguageContext'

export function LanguageToggle() {
  const { language, setLanguage } = useLanguage()

  return (
    <div className="inline-flex shrink-0 rounded-full bg-pink-50 p-0.5 text-xs font-semibold">
      <button
        type="button"
        onClick={() => setLanguage('th')}
        aria-pressed={language === 'th'}
        className={`rounded-full px-2.5 py-1 transition-colors ${
          language === 'th' ? 'bg-pink-500 text-white' : 'text-pink-700 hover:bg-pink-100'
        }`}
      >
        ไทย
      </button>
      <button
        type="button"
        onClick={() => setLanguage('en')}
        aria-pressed={language === 'en'}
        className={`rounded-full px-2.5 py-1 transition-colors ${
          language === 'en' ? 'bg-pink-500 text-white' : 'text-pink-700 hover:bg-pink-100'
        }`}
      >
        EN
      </button>
    </div>
  )
}
