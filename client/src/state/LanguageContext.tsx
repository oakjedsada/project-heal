import { createContext, useContext, useState, type ReactNode } from 'react'
import { translations, type Language, type TranslationKey } from '../i18n/translations'

const LANGUAGE_KEY = 'mindcheck.language'

function loadLanguage(): Language {
  try {
    return window.localStorage.getItem(LANGUAGE_KEY) === 'en' ? 'en' : 'th'
  } catch {
    return 'th'
  }
}

interface LanguageContextValue {
  language: Language
  setLanguage: (language: Language) => void
  t: (key: TranslationKey, params?: Record<string, string | number>) => string
}

const LanguageContext = createContext<LanguageContextValue | null>(null)

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState<Language>(() => loadLanguage())

  const setLanguage = (next: Language) => {
    setLanguageState(next)
    try {
      window.localStorage.setItem(LANGUAGE_KEY, next)
    } catch {
      // ignore — language just won't persist across reloads
    }
  }

  const t = (key: TranslationKey, params?: Record<string, string | number>): string => {
    let text = translations[language][key]
    if (params) {
      for (const [name, value] of Object.entries(params)) {
        text = text.replace(`{{${name}}}`, String(value))
      }
    }
    return text
  }

  return <LanguageContext.Provider value={{ language, setLanguage, t }}>{children}</LanguageContext.Provider>
}

export function useLanguage(): LanguageContextValue {
  const ctx = useContext(LanguageContext)
  if (!ctx) {
    throw new Error('useLanguage must be used within LanguageProvider')
  }
  return ctx
}
