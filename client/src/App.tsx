import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminRouteGuard } from './admin/AdminRouteGuard'
import { CreateInstrumentPage } from './admin/pages/CreateInstrumentPage'
import { DashboardPage } from './admin/pages/DashboardPage'
import { EditUserPage } from './admin/pages/EditUserPage'
import { FlowTransitionsPage } from './admin/pages/FlowTransitionsPage'
import { UserManagementPage } from './admin/pages/UserManagementPage'
import { Layout } from './components/Layout'
import { AssessmentPage } from './pages/AssessmentPage'
import { ConsentPage } from './pages/ConsentPage'
import { EmergencyPage } from './pages/EmergencyPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { ResultPage } from './pages/ResultPage'
import { AuthProvider } from './state/AuthContext'
import { AuthRouteGuard } from './state/AuthRouteGuard'
import { SessionProvider } from './state/SessionContext'

function PublicApp() {
  return (
    <SessionProvider>
      <Layout>
        <Routes>
          <Route
            path="/"
            element={
              <AuthRouteGuard>
                <ConsentPage />
              </AuthRouteGuard>
            }
          />
          <Route
            path="/assessment"
            element={
              <AuthRouteGuard>
                <AssessmentPage />
              </AuthRouteGuard>
            }
          />
          <Route
            path="/result"
            element={
              <AuthRouteGuard>
                <ResultPage />
              </AuthRouteGuard>
            }
          />
          <Route
            path="/emergency"
            element={
              <AuthRouteGuard>
                <EmergencyPage />
              </AuthRouteGuard>
            }
          />
        </Routes>
      </Layout>
    </SessionProvider>
  )
}

function AdminApp() {
  return (
    <Routes>
      <Route
        path="dashboard"
        element={
          <AdminRouteGuard>
            <DashboardPage />
          </AdminRouteGuard>
        }
      />
      <Route
        path="instruments/new"
        element={
          <AdminRouteGuard>
            <CreateInstrumentPage />
          </AdminRouteGuard>
        }
      />
      <Route
        path="flow-transitions"
        element={
          <AdminRouteGuard>
            <FlowTransitionsPage />
          </AdminRouteGuard>
        }
      />
      <Route
        path="users"
        element={
          <AdminRouteGuard>
            <UserManagementPage />
          </AdminRouteGuard>
        }
      />
      <Route
        path="users/:id"
        element={
          <AdminRouteGuard>
            <EditUserPage />
          </AdminRouteGuard>
        }
      />
      <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
    </Routes>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/admin/*" element={<AdminApp />} />
        <Route path="/*" element={<PublicApp />} />
      </Routes>
    </AuthProvider>
  )
}
