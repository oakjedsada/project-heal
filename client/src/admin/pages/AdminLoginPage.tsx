import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAdminAuth } from '../AdminAuthContext'

export function AdminLoginPage() {
  const navigate = useNavigate()
  const { login } = useAdminAuth()
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)
    const ok = await login(password)
    setIsLoading(false)
    if (ok) {
      navigate('/admin/dashboard')
    } else {
      setError('รหัสผ่านไม่ถูกต้อง')
    }
  }

  return (
    <div className="mx-auto max-w-sm px-4 py-10">
      <h1 className="mb-4 text-xl font-semibold text-slate-900">MindCheck Admin</h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-3">
        <label htmlFor="admin-password" className="text-sm font-medium text-slate-700">
          รหัสผ่าน admin
        </label>
        <input
          id="admin-password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="min-h-11 rounded-md border border-slate-300 px-3 py-2"
          autoFocus
        />
        {error && (
          <p role="alert" className="text-sm text-red-700">
            {error}
          </p>
        )}
        <button
          type="submit"
          disabled={isLoading}
          className="min-h-11 rounded-md bg-blue-700 px-4 py-3 font-medium text-white hover:bg-blue-800 disabled:opacity-60"
        >
          {isLoading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ'}
        </button>
      </form>
    </div>
  )
}
