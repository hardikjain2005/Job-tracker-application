import { useState, type ReactNode } from 'react'
import { tokenStore } from '../api'
import { AuthContext, type AuthState } from './authContext'

export function AuthProvider({ children }: { children: ReactNode }) {
  // Initialise from localStorage so a page refresh keeps you logged in.
  const [token, setToken] = useState(tokenStore.get())
  const [email, setEmail] = useState(tokenStore.getEmail())

  const value: AuthState = {
    email,
    isAuthenticated: token !== null,
    signIn: ({ token, email }) => {
      tokenStore.set(token, email)
      setToken(token)
      setEmail(email)
    },
    signOut: () => {
      tokenStore.clear()
      setToken(null)
      setEmail(null)
    },
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
