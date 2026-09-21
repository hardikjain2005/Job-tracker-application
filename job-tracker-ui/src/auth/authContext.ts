import { createContext } from 'react'
import type { LoginResponse } from '../types'

export interface AuthState {
  email: string | null
  isAuthenticated: boolean
  signIn: (response: LoginResponse) => void
  signOut: () => void
}

export const AuthContext = createContext<AuthState | null>(null)
