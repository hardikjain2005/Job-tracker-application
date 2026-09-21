import axios, { isAxiosError } from 'axios'

const TOKEN_KEY = 'token'
const EMAIL_KEY = 'email'

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  getEmail: () => localStorage.getItem(EMAIL_KEY),
  set: (token: string, email: string) => {
    localStorage.setItem(TOKEN_KEY, token)
    localStorage.setItem(EMAIL_KEY, email)
  },
  clear: () => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(EMAIL_KEY)
  },
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5176',
})

// Attach the JWT to every request.
api.interceptors.request.use((config) => {
  const token = tokenStore.get()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// An expired or invalid token on a protected call means we must log in again.
// (A 401 from /login itself just means "wrong password", so it is left alone.)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthCall = String(error.config?.url ?? '').startsWith('/api/auth/')
    if (isAxiosError(error) && error.response?.status === 401 && !isAuthCall) {
      tokenStore.clear()
      window.location.assign('/login')
    }
    return Promise.reject(error)
  },
)

/** Turns an API error (message, ASP.NET validation errors, or network failure) into text for the user. */
export function getErrorMessage(error: unknown): string {
  if (isAxiosError(error)) {
    if (!error.response) return 'Cannot reach the server. Is the API running?'
    const data = error.response.data
    if (data?.message) return data.message
    if (data?.errors) return Object.values<string[]>(data.errors).flat().join(' ')
    if (data?.title) return data.title
  }
  return 'Something went wrong.'
}
