import { createContext, useContext, useState, type ReactNode } from 'react'
import { apiClient } from '../api/client'

const ADMIN_TOKEN_KEY = 'mindcheck.admin.token'

// sessionStorage (not localStorage): an admin credential should not outlive
// the browser tab, unlike the public assessment's resume-friendly progress.
function loadToken(): string | null {
  try {
    return window.sessionStorage.getItem(ADMIN_TOKEN_KEY)
  } catch {
    return null
  }
}

function saveToken(token: string | null) {
  try {
    if (token) {
      window.sessionStorage.setItem(ADMIN_TOKEN_KEY, token)
    } else {
      window.sessionStorage.removeItem(ADMIN_TOKEN_KEY)
    }
  } catch {
    // ignore
  }
}

interface AdminAuthContextValue {
  token: string | null
  authHeader: Record<string, string>
  login: (password: string) => Promise<boolean>
  logout: () => void
}

const AdminAuthContext = createContext<AdminAuthContextValue | null>(null)

export function AdminAuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => loadToken())

  const login = async (password: string): Promise<boolean> => {
    const { data, error } = await apiClient.POST('/api/admin/auth/login', {
      body: { password },
    })
    if (error || !data?.token) {
      return false
    }
    saveToken(data.token)
    setToken(data.token)
    return true
  }

  const logout = () => {
    saveToken(null)
    setToken(null)
  }

  const authHeader = token ? { Authorization: `Bearer ${token}` } : {}

  return (
    <AdminAuthContext.Provider value={{ token, authHeader, login, logout }}>
      {children}
    </AdminAuthContext.Provider>
  )
}

export function useAdminAuth(): AdminAuthContextValue {
  const ctx = useContext(AdminAuthContext)
  if (!ctx) {
    throw new Error('useAdminAuth must be used within AdminAuthProvider')
  }
  return ctx
}
