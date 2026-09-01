import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminAuthProvider } from './admin/AdminAuthContext'
import { AdminRouteGuard } from './admin/AdminRouteGuard'
import { AdminLoginPage } from './admin/pages/AdminLoginPage'
import { CreateInstrumentPage } from './admin/pages/CreateInstrumentPage'
import { DashboardPage } from './admin/pages/DashboardPage'
import { FlowTransitionsPage } from './admin/pages/FlowTransitionsPage'
import { Layout } from './components/Layout'
import { AssessmentPage } from './pages/AssessmentPage'
import { ConsentPage } from './pages/ConsentPage'
import { EmergencyPage } from './pages/EmergencyPage'
import { ResultPage } from './pages/ResultPage'
import { SessionProvider } from './state/SessionContext'

function PublicApp() {
  return (
    <SessionProvider>
      <Layout>
        <Routes>
          <Route path="/" element={<ConsentPage />} />
          <Route path="/assessment" element={<AssessmentPage />} />
          <Route path="/result" element={<ResultPage />} />
          <Route path="/emergency" element={<EmergencyPage />} />
        </Routes>
      </Layout>
    </SessionProvider>
  )
}

function AdminApp() {
  return (
    <AdminAuthProvider>
      <Routes>
        <Route path="login" element={<AdminLoginPage />} />
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
        <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
      </Routes>
    </AdminAuthProvider>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/admin/*" element={<AdminApp />} />
      <Route path="/*" element={<PublicApp />} />
    </Routes>
  )
}
