import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

/** Renders the nested routes only when logged in; otherwise redirects to /login. */
export default function ProtectedRoute() {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />
}
