import type { ReactElement } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { session } from './lib/api'
import LoginPage from './pages/LoginPage'
import HomePage from './pages/HomePage'

function RequireAuth({ children }: { children: ReactElement }) {
  return session.token ? children : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<RequireAuth><HomePage /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
