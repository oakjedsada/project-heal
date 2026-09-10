import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { SparkleIcon } from '../components/icons'
import { useAuth } from '../state/AuthContext'

export function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuth()
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
      setError('ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง')
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <span className="mb-3 inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
        <SparkleIcon className="h-5 w-5" />
      </span>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">MindCheck</h1>
      <p className="mb-6 text-sm text-stone-500">เข้าสู่ระบบเพื่อทำแบบประเมินหรือจัดการระบบ</p>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
      >
        <label htmlFor="login-username" className="text-sm font-medium text-stone-700">
          ชื่อผู้ใช้หรืออีเมล
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
            รหัสผ่าน
          </label>
          <Link to="/forgot-password" className="text-xs font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
            ลืมรหัสผ่าน?
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
          {isLoading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-stone-500">
        ยังไม่มีบัญชี?{' '}
        <Link to="/register" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
          สมัครสมาชิก
        </Link>
      </p>
    </div>
  )
}
