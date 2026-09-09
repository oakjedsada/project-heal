import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../../state/AuthContext'

const NAV_ITEMS = [
  { to: '/admin/dashboard', label: 'สถิติรวม' },
  { to: '/admin/instruments/new', label: 'สร้างแบบประเมิน' },
  { to: '/admin/flow-transitions', label: 'เส้นทางแบบประเมิน' },
  { to: '/admin/users', label: 'จัดการผู้ใช้' },
]

export function AdminHeader({ title }: { title: string }) {
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="mb-6">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-1">
        <h1 className="text-xl font-semibold tracking-tight text-stone-900">{title}</h1>
        <span className="text-xs text-stone-500">
          เข้าสู่ระบบเป็น {user?.username} ·{' '}
          <button
            type="button"
            onClick={handleLogout}
            className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700"
          >
            ออกจากระบบ
          </button>
        </span>
      </div>
      <nav className="flex flex-wrap gap-1 rounded-full bg-pink-50 p-1 text-sm">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `rounded-full px-3 py-1.5 font-medium transition-colors ${
                isActive ? 'bg-pink-500 text-white shadow-sm' : 'text-pink-700 hover:bg-pink-100'
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
