import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../state/AuthContext'
import { useLanguage } from '../state/LanguageContext'
import { SparkleIcon } from './icons'
import { LanguageToggle } from './LanguageToggle'

export function Layout({ children }: { children: ReactNode }) {
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
    <div className="flex min-h-screen flex-col">
      <div className="border-b border-amber-200/80 bg-amber-50 px-4 py-2.5 text-center text-sm text-amber-800">
        {t('layout.demoBannerBefore')}
        <strong className="font-semibold">{t('layout.demoBannerBold')}</strong>
        {t('layout.demoBannerAfter')}
      </div>
      <header className="mx-auto flex w-full max-w-lg flex-wrap items-center justify-between gap-x-3 gap-y-1 px-5 pt-8">
        <span className="flex items-center gap-1.5 text-lg font-semibold tracking-tight text-pink-600">
          <SparkleIcon className="h-4 w-4" />
          MindCheck
        </span>
        <div className="flex items-center gap-3">
          {user && (
            <span className="text-xs text-stone-500">
              {t('common.loggedInAs', { username: user.username })} ·{' '}
              <button type="button" onClick={handleLogout} className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700">
                {t('common.logout')}
              </button>{' '}
              ·{' '}
              <button type="button" onClick={handleLogoutAll} className="underline decoration-stone-300 underline-offset-2 hover:text-stone-700">
                {t('common.logoutAll')}
              </button>
            </span>
          )}
          <LanguageToggle />
        </div>
      </header>
      <main className="mx-auto w-full max-w-lg flex-1 px-5 py-6">{children}</main>
    </div>
  )
}
