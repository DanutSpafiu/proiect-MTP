import { Routes, Route, Navigate } from 'react-router-dom'
import { getToken } from './api'
import LoginPage from './pages/LoginPage.jsx'
import DashboardPage from './pages/DashboardPage.jsx'
import StudentDetailPage from './pages/StudentDetailPage.jsx'

function RequireAuth({ children }) {
  return getToken() ? children : <Navigate to="/" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<LoginPage />} />
      <Route path="/dashboard" element={<RequireAuth><DashboardPage /></RequireAuth>} />
      <Route path="/students/:id" element={<RequireAuth><StudentDetailPage /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
