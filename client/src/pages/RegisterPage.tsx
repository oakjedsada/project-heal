import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { HeartIcon } from '../components/icons'
import { useAuth } from '../state/AuthContext'

export function RegisterPage() {
  const navigate = useNavigate()
  const { register } = useAuth()
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
      setError('รหัสผ่านทั้งสองช่องไม่ตรงกัน')
      return
    }

    setIsLoading(true)
    const user = await register(username, email, password)
    setIsLoading(false)
    if (user) {
      navigate('/')
    } else {
      setError('สมัครสมาชิกไม่สำเร็จ ชื่อผู้ใช้/อีเมลอาจถูกใช้แล้ว อีเมลไม่ถูกต้อง หรือรหัสผ่านสั้นเกินไป (อย่างน้อย 8 ตัวอักษร)')
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <span className="mb-3 inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
        <HeartIcon className="h-5 w-5" />
      </span>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">MindCheck</h1>
      <p className="mb-6 text-sm text-stone-500">สมัครสมาชิกเพื่อเริ่มทำแบบประเมิน</p>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
      >
        <label htmlFor="register-username" className="text-sm font-medium text-stone-700">
          ชื่อผู้ใช้ (อย่างน้อย 3 ตัวอักษร)
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
          อีเมล
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
          รหัสผ่าน (อย่างน้อย 8 ตัวอักษร)
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
          ยืนยันรหัสผ่าน
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
          {isLoading ? 'กำลังสมัครสมาชิก...' : 'สมัครสมาชิก'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-stone-500">
        มีบัญชีอยู่แล้ว?{' '}
        <Link to="/login" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
          เข้าสู่ระบบ
        </Link>
      </p>
    </div>
  )
}
