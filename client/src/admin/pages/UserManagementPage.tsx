import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'

type UserDto = components['schemas']['UserDto']

const ROLES = ['User', 'Admin']

export function UserManagementPage() {
  const { authHeader } = useAuth()
  const { t, language } = useLanguage()

  const [users, setUsers] = useState<UserDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('User')
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadUsers = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    const { data, error: apiError } = await apiClient.GET('/api/admin/users', { headers: authHeader })
    if (apiError) {
      setError(t('admin.users.loadFailed'))
    } else {
      setUsers(data ?? [])
    }
    setIsLoading(false)
  }, [authHeader, t])

  useEffect(() => {
    void loadUsers()
  }, [loadUsers])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setFormError(null)
    setIsSubmitting(true)

    const { error: apiError, response } = await apiClient.POST('/api/admin/users', {
      headers: authHeader,
      body: { username, email, password, role },
    })

    setIsSubmitting(false)

    if (apiError || !response.ok) {
      const detail = (apiError as { detail?: string } | undefined)?.detail
      setFormError(detail ?? t('admin.users.createFailed', { status: response.status }))
      return
    }

    setUsername('')
    setEmail('')
    setPassword('')
    setRole('User')
    await loadUsers()
  }

  const handleDelete = async (userId: string) => {
    setError(null)
    const { error: apiError, response } = await apiClient.DELETE('/api/admin/users/{id}', {
      params: { path: { id: userId } },
      headers: authHeader,
    })
    if (apiError || !response.ok) {
      const detail = (apiError as { detail?: string } | undefined)?.detail
      setError(detail ?? t('admin.users.deleteFailed', { status: response.status }))
      return
    }
    await loadUsers()
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <AdminHeader title={t('admin.users.title')} />

      {error && <p className="mb-4 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      <section className="mb-6 rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">{t('admin.users.addNew')}</h2>
        <form onSubmit={handleCreate} className="grid grid-cols-2 gap-3">
          <label className="flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.users.username')}
            <input
              required
              autoComplete="off"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className={inputClass}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.users.email')}
            <input
              required
              type="email"
              autoComplete="off"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={inputClass}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.users.password')}
            <input
              required
              type="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={inputClass}
            />
          </label>
          <label className="col-span-2 flex flex-col gap-1 text-sm text-stone-700">
            {t('admin.users.role')}
            <select value={role} onChange={(e) => setRole(e.target.value)} className={inputClass}>
              {ROLES.map((r) => (
                <option key={r} value={r}>
                  {r}
                </option>
              ))}
            </select>
          </label>

          {formError && <p className="col-span-2 text-sm text-red-700">{formError}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="col-span-2 min-h-11 rounded-full bg-pink-500 px-4 py-2 font-medium text-white transition-colors hover:bg-pink-600 disabled:opacity-60"
          >
            {isSubmitting ? t('admin.users.addSubmitBusy') : t('admin.users.addSubmit')}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">{t('admin.users.allUsers')}</h2>
        {isLoading ? (
          <p className="text-sm text-stone-500">{t('common.loading')}</p>
        ) : (
          <ul className="flex flex-col gap-2 text-sm">
            {users.map((u) => (
              <li
                key={u.userId}
                className="flex items-center justify-between gap-3 rounded-xl border border-stone-100 px-3 py-2"
              >
                <div className="flex flex-col">
                  <span className="font-medium text-stone-900">{u.username}</span>
                  <span className="text-xs text-stone-500">{u.email}</span>
                  <span className="text-xs text-stone-400">
                    {t('admin.users.createdAt', {
                      date: u.createdAt
                        ? new Date(u.createdAt).toLocaleString(language === 'th' ? 'th-TH' : 'en-US')
                        : '-',
                    })}
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <span className="rounded-full bg-pink-50 px-2.5 py-1 text-xs font-medium text-pink-700">
                    {u.role}
                  </span>
                  <Link
                    to={`/admin/users/${u.userId}`}
                    className="rounded-lg border border-pink-300 px-2 py-1 text-xs text-pink-700 hover:bg-pink-50"
                  >
                    {t('admin.users.edit')}
                  </Link>
                  <button
                    type="button"
                    onClick={() => handleDelete(u.userId!)}
                    className="rounded-lg border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50"
                  >
                    {t('admin.users.delete')}
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}

const inputClass = 'min-h-11 rounded-lg border border-stone-300 px-2 py-1 text-sm focus:border-pink-400'
