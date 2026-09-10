import { createContext, useContext, useState, type ReactNode } from 'react'
import { apiClient } from '../api/client'

const AUTH_KEY = 'mindcheck.auth'

export interface AuthUser {
  userId: string
  username: string
  role: 'Admin' | 'User'
}

interface StoredAuth {
  token: string
  user: AuthUser
}

function readStore(store: Storage): StoredAuth | null {
  try {
    const raw = store.getItem(AUTH_KEY)
    return raw ? (JSON.parse(raw) as StoredAuth) : null
  } catch {
    return null
  }
}

function loadAuth(): StoredAuth | null {
  try {
    return readStore(window.localStorage) ?? readStore(window.sessionStorage)
  } catch {
    return null
  }
}

// Admin tokens shouldn't outlive the tab (the original reasoning behind the
// old admin-only sessionStorage choice); regular users taking a screening
// tool expect to stay logged in across visits, so they get localStorage.
function storeFor(role: AuthUser['role']): Storage {
  return role === 'Admin' ? window.sessionStorage : window.localStorage
}

function saveAuth(auth: StoredAuth | null) {
  try {
    window.localStorage.removeItem(AUTH_KEY)
    window.sessionStorage.removeItem(AUTH_KEY)
    if (auth) {
      storeFor(auth.user.role).setItem(AUTH_KEY, JSON.stringify(auth))
    }
  } catch {
    // ignore
  }
}

interface AuthTokenResponseLike {
  token?: string
  userId?: string
  username?: string
  role?: string
}

interface AuthContextValue {
  token: string | null
  user: AuthUser | null
  authHeader: Record<string, string>
  register: (username: string, email: string, password: string) => Promise<AuthUser | null>
  login: (usernameOrEmail: string, password: string) => Promise<AuthUser | null>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<StoredAuth | null>(() => loadAuth())

  // Returns the just-authenticated user directly (not just a success flag) so
  // callers can act on the role (e.g. redirect target) without waiting for a
  // re-render to see the updated context value.
  const applyResponse = (data: AuthTokenResponseLike): AuthUser | null => {
    if (!data.token || !data.userId || !data.username || !data.role) {
      return null
    }
    const user: AuthUser = { userId: data.userId, username: data.username, role: data.role as AuthUser['role'] }
    const next: StoredAuth = { token: data.token, user }
    saveAuth(next)
    setAuth(next)
    return user
  }

  const register = async (username: string, email: string, password: string): Promise<AuthUser | null> => {
    const { data, error } = await apiClient.POST('/api/auth/register', { body: { username, email, password } })
    if (error || !data) {
      return null
    }
    return applyResponse(data)
  }

  const login = async (usernameOrEmail: string, password: string): Promise<AuthUser | null> => {
    const { data, error } = await apiClient.POST('/api/auth/login', { body: { usernameOrEmail, password } })
    if (error || !data) {
      return null
    }
    return applyResponse(data)
  }

  const logout = () => {
    saveAuth(null)
    setAuth(null)
  }

  const authHeader = auth ? { Authorization: `Bearer ${auth.token}` } : {}

  return (
    <AuthContext.Provider
      value={{ token: auth?.token ?? null, user: auth?.user ?? null, authHeader, register, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
