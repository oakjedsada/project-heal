import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../state/AuthContext'
import { SparkleIcon } from './icons'

export function Layout({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="flex min-h-screen flex-col">
      <div className="border-b border-amber-200/80 bg-amber-50 px-4 py-2.5 text-center text-sm text-amber-800">
        โปรเจกต์นี้เป็น<strong className="font-semibold">โปรเจกต์สาธิต (demo)</strong>{' '}
        เพื่อแสดงทักษะการออกแบบระบบเท่านั้น ไม่ใช่เครื่องมือวินิจฉัยทางการแพทย์
      </div>
      <header className="mx-auto flex w-full max-w-lg flex-wrap items-center justify-between gap-x-3 gap-y-1 px-5 pt-8">
        <span className="flex items-center gap-1.5 text-lg font-semibold tracking-tight text-pink-600">
          <SparkleIcon className="h-4 w-4" />
          MindCheck
        </span>
        {user && (
          <span className="text-xs text-stone-500">
            เข้าสู่ระบบเป็น {user.username} ·{' '}
            <button type="button" onClick={handleLogout} className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700">
              ออกจากระบบ
            </button>
          </span>
        )}
      </header>
      <main className="mx-auto w-full max-w-lg flex-1 px-5 py-6">{children}</main>
    </div>
  )
}
