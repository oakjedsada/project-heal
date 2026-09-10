import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { apiClient } from '../api/client'
import { HeartIcon } from '../components/icons'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const token = searchParams.get('token') ?? ''

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
    const { error: apiError, response } = await apiClient.POST('/api/auth/reset-password', {
      body: { token, newPassword: password },
    })
    setIsLoading(false)

    if (apiError || !response.ok) {
      setError('ลิงก์นี้ไม่ถูกต้องหรือหมดอายุแล้ว ลองขอลิงก์ใหม่อีกครั้ง')
      return
    }

    navigate('/login')
  }

  if (!token) {
    return (
      <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
        <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">ไม่พบลิงก์รีเซ็ตรหัสผ่านที่ถูกต้อง</p>
        <p className="mt-4 text-center text-sm text-stone-500">
          <Link to="/forgot-password" className="font-medium text-pink-600 underline decoration-pink-300 underline-offset-2">
            ขอลิงก์รีเซ็ตรหัสผ่านใหม่
          </Link>
        </p>
      </div>
    )
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-sm flex-col justify-center px-4 py-10">
      <span className="mb-3 inline-flex h-11 w-11 items-center justify-center rounded-2xl bg-pink-100 text-pink-500">
        <HeartIcon className="h-5 w-5" />
      </span>
      <h1 className="mb-1 text-xl font-semibold tracking-tight text-stone-900">ตั้งรหัสผ่านใหม่</h1>
      <p className="mb-6 text-sm text-stone-500">กรอกรหัสผ่านใหม่สำหรับบัญชีของคุณ</p>

      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-3 rounded-2xl border border-pink-100 bg-white p-5 shadow-sm shadow-pink-900/5"
      >
        <label htmlFor="reset-password" className="text-sm font-medium text-stone-700">
          รหัสผ่านใหม่ (อย่างน้อย 8 ตัวอักษร)
        </label>
        <input
          id="reset-password"
          type="password"
          required
          autoComplete="new-password"
          autoFocus
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="min-h-11 rounded-xl border border-stone-300 px-3 py-2 focus:border-pink-400"
        />

        <label htmlFor="reset-confirm-password" className="text-sm font-medium text-stone-700">
          ยืนยันรหัสผ่านใหม่
        </label>
        <input
          id="reset-confirm-password"
          type="password"
          required
          autoComplete="new-password"
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
          {isLoading ? 'กำลังบันทึก...' : 'ตั้งรหัสผ่านใหม่'}
        </button>
      </form>
    </div>
  )
}
