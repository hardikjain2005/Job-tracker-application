import { useState, type FormEvent } from 'react'
import { api, getErrorMessage } from '../api'
import { STATUSES, type ApplicationInput, type ApplicationStatus, type JobApplication } from '../types'

interface Props {
  /** When set, the form edits this application; otherwise it creates a new one. */
  editing: JobApplication | null
  onSaved: () => void
  onCancel: () => void
}

const today = () => new Date().toLocaleDateString('en-CA') // YYYY-MM-DD in local time

export default function ApplicationForm({ editing, onSaved, onCancel }: Props) {
  const [company, setCompany] = useState(editing?.company ?? '')
  const [role, setRole] = useState(editing?.role ?? '')
  const [status, setStatus] = useState<ApplicationStatus>(editing?.status ?? 'Applied')
  const [appliedDate, setAppliedDate] = useState(editing?.appliedDate ?? today())
  const [notes, setNotes] = useState(editing?.notes ?? '')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSaving(true)
    const body: ApplicationInput = {
      company: company.trim(),
      role: role.trim(),
      status,
      appliedDate,
      notes: notes.trim() || null,
    }
    try {
      if (editing) await api.put(`/api/applications/${editing.id}`, body)
      else await api.post('/api/applications', body)
      onSaved()
    } catch (err) {
      setError(getErrorMessage(err))
      setSaving(false)
    }
  }

  return (
    <form className="card form-grid" onSubmit={handleSubmit}>
      <h2>{editing ? 'Edit application' : 'Add application'}</h2>

      <label>
        Company
        <input value={company} onChange={(e) => setCompany(e.target.value)} maxLength={200} required />
      </label>

      <label>
        Role
        <input value={role} onChange={(e) => setRole(e.target.value)} maxLength={200} required />
      </label>

      <label>
        Status
        <select value={status} onChange={(e) => setStatus(e.target.value as ApplicationStatus)}>
          {STATUSES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
      </label>

      <label>
        Applied date
        <input type="date" value={appliedDate} onChange={(e) => setAppliedDate(e.target.value)} required />
      </label>

      <label className="wide">
        Notes
        <textarea value={notes} onChange={(e) => setNotes(e.target.value)} maxLength={2000} rows={3} />
      </label>

      {error && <p className="error wide" role="alert">{error}</p>}

      <div className="actions wide">
        <button type="submit" className="primary" disabled={saving}>
          {saving ? 'Saving…' : editing ? 'Save changes' : 'Add'}
        </button>
        <button type="button" onClick={onCancel} disabled={saving}>Cancel</button>
      </div>
    </form>
  )
}
