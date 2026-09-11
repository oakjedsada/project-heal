import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LanguageToggle } from '../components/LanguageToggle'
import { HeartIcon } from '../components/icons'
import { useAuth } from '../state/AuthContext'
import { useLanguage } from '../state/LanguageContext'

export function RegisterPage() {
  const navigate = useNavigate()
  const { register } = useAuth()
  const { t } = useLanguage()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (password !== confirmPassword) {
      setError(t('register.passwordMismatch'))
      return
    }

    setIsLoading(true)
    const result = await register(username, email, password)
    setIsLoading(false)
    if (result.ok === true) {
      navigate('/')
    } else if (result.status === 429) {
      setError(t('login.rateLimited'))
    } else {
      setError(t('register.failedGeneric'))
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <div className="mb-3 flex items-center justify-between">
        <span className="inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
          <HeartIcon className="h-5 w-5" />
        </span>
        <LanguageToggle />
      </div>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">MindCheck</h1>
      <p className="mb-6 text-sm text-stone-500">{t('register.subtitle')}</p>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
      >
        <label htmlFor="register-username" className="text-sm font-medium text-stone-700">
          {t('register.username')}
        </label>
        <input
          id="register-username"
          type="text"
          autoComplete="username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
          autoFocus
        />

        <label htmlFor="register-email" className="text-sm font-medium text-stone-700">
          {t('register.email')}
        </label>
        <input
          id="register-email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
        />

        <label htmlFor="register-password" className="text-sm font-medium text-stone-700">
          {t('register.password')}
        </label>
        <input
          id="register-password"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
        />

        <label htmlFor="register-confirm-password" className="text-sm font-medium text-stone-700">
          {t('register.confirmPassword')}
        </label>
        <input
          id="register-confirm-password"
          autoComplete="new-password"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
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
          {isLoading ? t('register.submitBusy') : t('register.submit')}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-stone-500">
        {t('register.haveAccount')}{' '}
        <Link to="/login" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
          {t('register.loginLink')}
        </Link>
      </p>
    </div>
  )
}
