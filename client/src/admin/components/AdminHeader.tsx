import { NavLink, useNavigate } from 'react-router-dom'
import { LanguageToggle } from '../../components/LanguageToggle'
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import type { TranslationKey } from '../../i18n/translations'

// `end` defaults to false (NavLink highlights on any matching prefix, so a
// sub-page like /admin/users/:id still lights up "จัดการผู้ใช้"); only
// /admin/instruments needs it set since /admin/instruments/new would
// otherwise also match its prefix and highlight both pills at once.
const NAV_ITEMS: { to: string; labelKey: TranslationKey; end?: boolean }[] = [
  { to: '/admin/dashboard', labelKey: 'admin.nav.dashboard' },
  { to: '/admin/instruments/new', labelKey: 'admin.nav.createInstrument' },
  { to: '/admin/instruments', labelKey: 'admin.nav.instruments', end: true },
  { to: '/admin/flow-transitions', labelKey: 'admin.nav.flowTransitions' },
  { to: '/admin/users', labelKey: 'admin.nav.users' },
]

export function AdminHeader({ title }: { title: string }) {
  const navigate = useNavigate()
  const { user, logout, logoutAllDevices } = useAuth()
  const { t } = useLanguage()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const handleLogoutAll = async () => {
    await logoutAllDevices()
    navigate('/login')
  }

  return (
    <div className="mb-6">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-1">
        <h1 className="text-xl font-semibold tracking-tight text-stone-900">{title}</h1>
        <div className="flex items-center gap-3">
          <span className="text-xs text-stone-500">
            {t('common.loggedInAs', { username: user?.username ?? '' })} ·{' '}
            <button
              type="button"
              onClick={handleLogout}
              className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700"
            >
              {t('common.logout')}
            </button>{' '}
            ·{' '}
            <button
              type="button"
              onClick={handleLogoutAll}
              className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700"
            >
              {t('common.logoutAll')}
            </button>
          </span>
          <LanguageToggle />
        </div>
      </div>
      <nav className="flex flex-wrap gap-1 rounded-full bg-pink-50 p-1 text-sm">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `rounded-full px-3 py-1.5 font-medium transition-colors ${
                isActive ? 'bg-pink-500 text-white shadow-sm' : 'text-pink-700 hover:bg-pink-100'
              }`
            }
          >
            {t(item.labelKey)}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
