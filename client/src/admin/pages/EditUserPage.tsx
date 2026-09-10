import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '../../api/client'
import { useAuth } from '../../state/AuthContext'
import { AdminHeader } from '../components/AdminHeader'

const ROLES = ['User', 'Admin']

export function EditUserPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { authHeader } = useAuth()

  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [role, setRole] = useState('User')
  const [password, setPassword] = useState('')
  const [createdAt, setCreatedAt] = useState<string | null>(null)

  const [formError, setFormError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
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
          setLoadError('โหลดข้อมูลผู้ใช้ไม่สำเร็จ')
          return
        }
        setUsername(data.username ?? '')
        setEmail(data.email ?? '')
        setRole(data.role ?? 'User')
        setCreatedAt(data.createdAt ?? null)
      })
      .finally(() => setIsLoading(false))
  }, [id, authHeader])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return
    setFormError(null)
    setSuccessMessage(null)
    setIsSaving(true)

    const { error, response } = await apiClient.PATCH('/api/admin/users/{id}', {
      params: { path: { id } },
      headers: authHeader,
      body: { username, email, role, password: password || null },
    })

    setIsSaving(false)

    if (error || !response.ok) {
      const detail = (error as { detail?: string } | undefined)?.detail
      setFormError(detail ?? `บันทึกไม่สำเร็จ (${response.status})`)
      return
    }

    setPassword('')
    setSuccessMessage('บันทึกข้อมูลผู้ใช้แล้ว')
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
      setFormError(detail ?? `ลบผู้ใช้ไม่สำเร็จ (${response.status})`)
      return
    }

    navigate('/admin/users')
  }

  return (
    <div className="mx-auto max-w-xl px-4 py-8">
      <AdminHeader title="แก้ไขข้อมูลผู้ใช้" />

      {isLoading && <p className="text-stone-500">กำลังโหลด...</p>}
      {notFound && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">ไม่พบผู้ใช้นี้</p>}
      {loadError && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{loadError}</p>}

      {!isLoading && !notFound && !loadError && (
        <>
          <form
            onSubmit={handleSubmit}
            className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
          >
            <label className="flex flex-col gap-1 text-sm text-stone-700">
              ชื่อผู้ใช้
              <input
                required
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={inputClass}
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-stone-700">
              อีเมล
              <input
                required
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={inputClass}
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-stone-700">
              Role
              <select value={role} onChange={(e) => setRole(e.target.value)} className={inputClass}>
                {ROLES.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm text-stone-700">
              ตั้งรหัสผ่านใหม่ (เว้นว่างไว้ถ้าไม่ต้องการเปลี่ยน)
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={inputClass}
                placeholder="อย่างน้อย 8 ตัวอักษร"
              />
            </label>

            {createdAt && (
              <p className="text-xs text-stone-400">สร้างเมื่อ {new Date(createdAt).toLocaleString('th-TH')}</p>
            )}

            {formError && <p className="text-sm text-red-700">{formError}</p>}
            {successMessage && <p className="text-sm text-pink-700">{successMessage}</p>}

            <div className="mt-2 flex gap-3">
              <button
                type="submit"
                disabled={isSaving}
                className="min-h-11 flex-1 rounded-full bg-pink-500 px-4 py-2 font-medium text-white transition-colors hover:bg-pink-600 disabled:opacity-60"
              >
                {isSaving ? 'กำลังบันทึก...' : 'บันทึกการเปลี่ยนแปลง'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/admin/users')}
                className="min-h-11 rounded-full border border-stone-300 bg-white px-4 py-2 font-medium text-stone-700 transition-colors hover:bg-stone-50"
              >
                ยกเลิก
              </button>
            </div>
          </form>

          <div className="mt-6 rounded-2xl border border-red-200 bg-red-50/50 p-4">
            <h2 className="mb-1 font-medium text-red-900">โซนอันตราย</h2>
            <p className="mb-3 text-sm text-red-700">ลบผู้ใช้นี้ออกจากระบบถาวร ไม่สามารถย้อนกลับได้</p>
            <button
              type="button"
              onClick={handleDelete}
              disabled={isDeleting}
              className="min-h-11 rounded-full border border-red-300 bg-white px-4 py-2 text-sm font-medium text-red-700 transition-colors hover:bg-red-50 disabled:opacity-60"
            >
              {isDeleting ? 'กำลังลบ...' : 'ลบผู้ใช้นี้'}
            </button>
          </div>
        </>
      )}
    </div>
  )
}

const inputClass = 'min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400'
