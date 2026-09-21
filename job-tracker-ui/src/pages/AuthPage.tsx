import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { api, getErrorMessage } from '../api'
import { useAuth } from '../auth/useAuth'
import type { LoginResponse } from '../types'

export default function AuthPage({ mode }: { mode: 'login' | 'register' }) {
  const { isAuthenticated, signIn } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const isRegister = mode === 'register'

  if (isAuthenticated) return <Navigate to="/" replace />

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      if (isRegister) await api.post('/api/auth/register', { email, password })
      const { data } = await api.post<LoginResponse>('/api/auth/login', { email, password })
      signIn(data)
      navigate('/', { replace: true })
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <main className="auth">
      <h1>Job Tracker</h1>
      <form className="card" onSubmit={handleSubmit}>
        <h2>{isRegister ? 'Create an account' : 'Log in'}</h2>

        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            required
          />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={isRegister ? 'new-password' : 'current-password'}
            minLength={isRegister ? 8 : undefined}
            required
          />
          {isRegister && <small>At least 8 characters.</small>}
        </label>

        {error && <p className="error" role="alert">{error}</p>}

        <button type="submit" className="primary" disabled={submitting}>
          {submitting ? 'Please wait…' : isRegister ? 'Register' : 'Log in'}
        </button>

        <p className="switch">
          {isRegister ? (
            <>Already have an account? <Link to="/login">Log in</Link></>
          ) : (
            <>New here? <Link to="/register">Create an account</Link></>
          )}
        </p>
      </form>
    </main>
  )
}
