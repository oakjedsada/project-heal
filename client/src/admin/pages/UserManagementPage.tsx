import { useCallback, useEffect, useState } from 'react'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAuth } from '../../state/AuthContext'
import { AdminHeader } from '../components/AdminHeader'

type UserDto = components['schemas']['UserDto']

const ROLES = ['User', 'Admin']

export function UserManagementPage() {
  const { authHeader } = useAuth()

  const [users, setUsers] = useState<UserDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('User')
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadUsers = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    const { data, error: apiError } = await apiClient.GET('/api/admin/users', { headers: authHeader })
    if (apiError) {
      setError('โหลดรายชื่อผู้ใช้ไม่สำเร็จ')
    } else {
      setUsers(data ?? [])
    }
    setIsLoading(false)
  }, [authHeader])

  useEffect(() => {
    void loadUsers()
  }, [loadUsers])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setFormError(null)
    setIsSubmitting(true)

    const { error: apiError, response } = await apiClient.POST('/api/admin/users', {
      headers: authHeader,
      body: { username, password, role },
    })

    setIsSubmitting(false)

    if (apiError || !response.ok) {
      const detail = (apiError as { detail?: string } | undefined)?.detail
      setFormError(detail ?? `สร้างผู้ใช้ไม่สำเร็จ (${response.status})`)
      return
    }

    setUsername('')
    setPassword('')
    setRole('User')
    await loadUsers()
  }

  const handleChangeRole = async (userId: string, newRole: string) => {
    setError(null)
    const { error: apiError, response } = await apiClient.PATCH('/api/admin/users/{id}/role', {
      params: { path: { id: userId } },
      headers: authHeader,
      body: { role: newRole },
    })
    if (apiError || !response.ok) {
      const detail = (apiError as { detail?: string } | undefined)?.detail
      setError(detail ?? `เปลี่ยน role ไม่สำเร็จ (${response.status})`)
      return
    }
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
      setError(detail ?? `ลบผู้ใช้ไม่สำเร็จ (${response.status})`)
      return
    }
    await loadUsers()
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <AdminHeader title="จัดการผู้ใช้" />

      {error && <p className="mb-4 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      <section className="mb-6 rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">เพิ่มผู้ใช้ใหม่</h2>
        <form onSubmit={handleCreate} className="grid grid-cols-2 gap-3">
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
            รหัสผ่าน
            <input
              required
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={inputClass}
            />
          </label>
          <label className="col-span-2 flex flex-col gap-1 text-sm text-stone-700">
            Role
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
            {isSubmitting ? 'กำลังบันทึก...' : 'เพิ่มผู้ใช้'}
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
        <h2 className="mb-3 font-medium text-stone-900">ผู้ใช้ทั้งหมด</h2>
        {isLoading ? (
          <p className="text-sm text-stone-500">กำลังโหลด...</p>
        ) : (
          <ul className="flex flex-col gap-2 text-sm">
            {users.map((u) => (
              <li
                key={u.userId}
                className="flex items-center justify-between gap-3 rounded-xl border border-stone-100 px-3 py-2"
              >
                <div className="flex flex-col">
                  <span className="font-medium text-stone-900">{u.username}</span>
                  <span className="text-xs text-stone-400">
                    สร้างเมื่อ {u.createdAt ? new Date(u.createdAt).toLocaleString('th-TH') : '-'}
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <select
                    value={u.role}
                    onChange={(e) => handleChangeRole(u.userId!, e.target.value)}
                    className="min-h-9 rounded-lg border border-stone-300 px-2 py-1 text-sm focus:border-pink-400"
                  >
                    {ROLES.map((r) => (
                      <option key={r} value={r}>
                        {r}
                      </option>
                    ))}
                  </select>
                  <button
                    type="button"
                    onClick={() => handleDelete(u.userId!)}
                    className="rounded-lg border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50"
                  >
                    ลบ
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
