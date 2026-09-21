export const STATUSES = ['Applied', 'Interview', 'Offer', 'Rejected'] as const
export type ApplicationStatus = (typeof STATUSES)[number]

export interface JobApplication {
  id: number
  company: string
  role: string
  status: ApplicationStatus
  appliedDate: string // "YYYY-MM-DD"
  notes: string | null
}

/** What the add/edit form sends; the server assigns the id. */
export interface ApplicationInput {
  company: string
  role: string
  status: ApplicationStatus
  appliedDate: string
  notes: string | null
}

export interface LoginResponse {
  token: string
  email: string
}
