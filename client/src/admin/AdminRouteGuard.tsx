import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../state/AuthContext'

export function AdminRouteGuard({ children }: { children: ReactNode }) {
  const { token, user } = useAuth()
  if (!token) {
    return <Navigate to="/login" replace />
  }
  if (user?.role !== 'Admin') {
    return <Navigate to="/" replace />
  }
  return <>{children}</>
}
