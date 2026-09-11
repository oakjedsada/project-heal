import { useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../api/client'
import { LanguageToggle } from '../components/LanguageToggle'
import { SparkleIcon } from '../components/icons'
import { useLanguage } from '../state/LanguageContext'

export function ForgotPasswordPage() {
  const { t } = useLanguage()
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitted, setSubmitted] = useState(false)
  const [devResetLink, setDevResetLink] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)

    const { data, error: apiError } = await apiClient.POST('/api/auth/forgot-password', { body: { email } })

    setIsLoading(false)

    if (apiError) {
      setError(t('forgotPassword.genericError'))
      return
    }

    setSubmitted(true)
    setDevResetLink(data?.devResetLink ?? null)
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <div className="mb-3 flex items-center justify-between">
        <span className="inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
          <SparkleIcon className="h-5 w-5" />
        </span>
        <LanguageToggle />
      </div>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">{t('forgotPassword.title')}</h1>
      <p className="mb-6 text-sm text-stone-500">{t('forgotPassword.subtitle')}</p>

      {!submitted ? (
        <form
          onSubmit={handleSubmit}
          className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
        >
          <label htmlFor="forgot-email" className="text-sm font-medium text-stone-700">
            {t('forgotPassword.email')}
          </label>
          <input
            id="forgot-email"
            type="email"
            required
            autoComplete="email"
            autoFocus
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
          />

          {error && (
            <p role="alert" className="text-sm text-red-700">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="min-h-11 rounded-full bg-pink-500 px-4 py-3 font-medium text-white transition-colors hover:bg-pink-600 disabled:opacity-60"
          >
            {isLoading ? t('forgotPassword.submitBusy') : t('forgotPassword.submit')}
          </button>
        </form>
      ) : (
        <div className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 text-sm leading-relaxed text-stone-600 shadow-sm shadow-pink-900/5">
          <p>{t('forgotPassword.successMessage')}</p>

          {devResetLink && (
            <div className="rounded-xl bg-amber-50 p-3 text-xs text-amber-900">
              <p className="mb-2 font-medium">{t('forgotPassword.devNotice')}</p>
              <Link to={devResetLink.replace(window.location.origin, '')} className="break-all font-medium underline">
                {devResetLink}
              </Link>
            </div>
          )}
        </div>
      )}

      <p className="mt-4 text-center text-sm text-stone-500">
        {t('forgotPassword.rememberedPassword')}{' '}
        <Link to="/login" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
          {t('forgotPassword.loginLink')}
        </Link>
      </p>
    </div>
  )
}
