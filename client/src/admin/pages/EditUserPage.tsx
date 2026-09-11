import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '../../api/client'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'

const ROLES = ['User', 'Admin']

export function EditUserPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { authHeader } = useAuth()
  const { t, language } = useLanguage()

  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [role, setRole] = useState('User')
  const [password, setPassword] = useState('')
  const [createdAt, setCreatedAt] = useState<string | null>(null)

  const [formError, setFormError] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)

  useEffect(() => {
    if (!id) return
    setIsLoading(true)
    setLoadError(null)
    apiClient
      .GET('/api/admin/users/{id}', { params: { path: { id } }, headers: authHeader })
      .then(({ data, error, response }) => {
        if (response.status === 404) {
          setNotFound(true)
          return
        }
        if (error || !data) {
          setLoadError(t('admin.editUser.loadFailed'))
          return
        }
        setUsername(data.username ?? '')
        setEmail(data.email ?? '')
        setRole(data.role ?? 'User')
        setCreatedAt(data.createdAt ?? null)
      })
      .finally(() => setIsLoading(false))
  }, [id, authHeader, t])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return
    setFormError(null)
    setIsSaving(true)

    const { error, response } = await apiClient.PATCH('/api/admin/users/{id}', {
      params: { path: { id } },
      headers: authHeader,
      body: { username, email, role, password: password || null },
    })

    setIsSaving(false)

    if (error || !response.ok) {
      const detail = (error as { detail?: string } | undefined)?.detail
      setFormError(detail ?? t('admin.editUser.saveFailed', { status: response.status }))
      return
    }

    navigate('/admin/users')
  }

  const handleDelete = async () => {
    if (!id) return
    setFormError(null)
    setIsDeleting(true)

    const { error, response } = await apiClient.DELETE('/api/admin/users/{id}', {
      params: { path: { id } },
      headers: authHeader,
    })

    setIsDeleting(false)

    if (error || !response.ok) {
      const detail = (error as { detail?: string } | undefined)?.detail
      setFormError(detail ?? t('admin.editUser.deleteFailed', { status: response.status }))
      return
    }

    navigate('/admin/users')
  }

  return (
    <div className="mx-auto max-w-xl px-4 py-8">
      <AdminHeader title={t('admin.editUser.title')} />

      {isLoading && <p className="text-stone-500">{t('common.loading')}</p>}
      {notFound && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{t('admin.editUser.notFound')}</p>}
      {loadError && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{loadError}</p>}

      {!isLoading && !notFound && !loadError && (
        <>
          <form
            onSubmit={handleSubmit}
            className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
          >
            <label className="flex flex-col gap-1 text-sm text-stone-700">
              {t('admin.editUser.username')}
              <input
                required
                autoComplete="off"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={inputClass}
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-stone-700">
              {t('admin.editUser.email')}
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
              {t('admin.editUser.role')}
              <select value={role} onChange={(e) => setRole(e.target.value)} className={inputClass}>
                {ROLES.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm text-stone-700">
              {t('admin.editUser.newPassword')}
              <input
                type="password"
                autoComplete="new-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={inputClass}
                placeholder={t('admin.editUser.newPasswordPlaceholder')}
              />
            </label>

            {createdAt && (
              <p className="text-xs text-stone-400">
                {t('admin.editUser.createdAt', {
                  date: new Date(createdAt).toLocaleString(language === 'th' ? 'th-TH' : 'en-US'),
                })}
              </p>
            )}

            {formError && <p className="text-sm text-red-700">{formError}</p>}

            <div className="mt-2 flex gap-3">
              <button
                type="submit"
                disabled={isSaving}
                className="min-h-11 flex-1 rounded-full bg-pink-500 px-4 py-2 font-medium text-white transition-colors hover:bg-pink-600 disabled:opacity-60"
              >
                {isSaving ? t('admin.editUser.saveBusy') : t('admin.editUser.save')}
              </button>
              <button
                type="button"
                onClick={() => navigate('/admin/users')}
                className="min-h-11 rounded-full border border-stone-300 bg-white px-4 py-2 font-medium text-stone-700 transition-colors hover:bg-stone-50"
              >
                {t('admin.editUser.cancel')}
              </button>
            </div>
          </form>

          <div className="mt-6 rounded-2xl border border-red-200 bg-red-50/50 p-4">
            <h2 className="mb-1 font-medium text-red-900">{t('admin.editUser.dangerZone')}</h2>
            <p className="mb-3 text-sm text-red-700">{t('admin.editUser.dangerDescription')}</p>
            <button
              type="button"
              onClick={handleDelete}
              disabled={isDeleting}
              className="min-h-11 rounded-full border border-red-300 bg-white px-4 py-2 text-sm font-medium text-red-700 transition-colors hover:bg-red-50 disabled:opacity-60"
            >
              {isDeleting ? t('admin.editUser.deleteBusy') : t('admin.editUser.deleteButton')}
            </button>
          </div>
        </>
      )}
    </div>
  )
}

const inputClass = 'min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400'
