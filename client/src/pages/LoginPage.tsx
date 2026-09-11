import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LanguageToggle } from '../components/LanguageToggle'
import { SparkleIcon } from '../components/icons'
import { useAuth } from '../state/AuthContext'
import { useLanguage } from '../state/LanguageContext'

export function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const { t } = useLanguage()
  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)
    const user = await login(usernameOrEmail, password)
    setIsLoading(false)
    if (user) {
      navigate(user.role === 'Admin' ? '/admin/dashboard' : '/')
    } else {
      setError(t('login.invalidCredentials'))
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <div className="mb-3 flex items-center justify-between">
        <span className="inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
          <SparkleIcon className="h-5 w-5" />
        </span>
        <LanguageToggle />
      </div>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">MindCheck</h1>
      <p className="mb-6 text-sm text-stone-500">{t('login.subtitle')}</p>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
      >
        <label htmlFor="login-username" className="text-sm font-medium text-stone-700">
          {t('login.usernameOrEmail')}
        </label>
        <input
          id="login-username"
          type="text"
          autoComplete="username"
          value={usernameOrEmail}
          onChange={(e) => setUsernameOrEmail(e.target.value)}
          className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
          autoFocus
        />

        <div className="flex items-center justify-between">
          <label htmlFor="login-password" className="text-sm font-medium text-stone-700">
            {t('login.password')}
          </label>
          <Link to="/forgot-password" className="text-xs font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
            {t('login.forgotPassword')}
          </Link>
        </div>
        <input
          id="login-password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
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
          {isLoading ? t('login.submitBusy') : t('login.submit')}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-stone-500">
        {t('login.noAccount')}{' '}
        <Link to="/register" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
          {t('login.registerLink')}
        </Link>
      </p>
    </div>
  )
}
