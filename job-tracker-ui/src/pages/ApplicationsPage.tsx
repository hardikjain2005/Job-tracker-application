import { useEffect, useState } from 'react'
import { api, getErrorMessage } from '../api'
import { useAuth } from '../auth/useAuth'
import ApplicationForm from '../components/ApplicationForm'
import { STATUSES, type ApplicationStatus, type JobApplication } from '../types'

export default function ApplicationsPage() {
  const { email, signOut } = useAuth()
  const [applications, setApplications] = useState<JobApplication[]>([])
  const [statusFilter, setStatusFilter] = useState<ApplicationStatus | ''>('')
  // true only until the first response; later refetches keep showing the previous rows meanwhile
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<JobApplication | null>(null)
  // Bumping this number re-runs the fetch effect, which is how we "refresh the list".
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let cancelled = false // ignore a slow response if the filter changed meanwhile
    api
      .get<JobApplication[]>('/api/applications', { params: statusFilter ? { status: statusFilter } : {} })
      .then(({ data }) => {
        if (cancelled) return
        setApplications(data)
        setError(null)
      })
      .catch((err) => !cancelled && setError(getErrorMessage(err)))
      .finally(() => !cancelled && setLoading(false))
    return () => {
      cancelled = true
    }
  }, [statusFilter, reloadKey])

  const refresh = () => setReloadKey((k) => k + 1)

  function openAdd() {
    setEditing(null)
    setShowForm(true)
  }

  function openEdit(app: JobApplication) {
    setEditing(app)
    setShowForm(true)
  }

  function closeForm() {
    setShowForm(false)
    setEditing(null)
  }

  function handleSaved() {
    closeForm()
    refresh()
  }

  async function handleDelete(app: JobApplication) {
    if (!window.confirm(`Delete the ${app.role} application at ${app.company}?`)) return
    try {
      await api.delete(`/api/applications/${app.id}`)
      refresh()
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  return (
    <div className="page">
      <header className="topbar">
        <h1>Job Tracker</h1>
        <div className="user">
          <span>{email}</span>
          <button onClick={signOut}>Log out</button>
        </div>
      </header>

      <div className="toolbar">
        <label className="inline">
          Status
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as ApplicationStatus | '')}>
            <option value="">All</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </label>
        {!showForm && <button className="primary" onClick={openAdd}>+ Add application</button>}
      </div>

      {showForm && (
        // key forces a fresh form (and fresh state) when switching between add / edit targets
        <ApplicationForm key={editing?.id ?? 'new'} editing={editing} onSaved={handleSaved} onCancel={closeForm} />
      )}

      {error && <p className="error" role="alert">{error}</p>}

      {loading && applications.length === 0 ? (
        <p className="muted">Loading…</p>
      ) : applications.length === 0 ? (
        <p className="muted">
          {statusFilter ? `No applications with status "${statusFilter}".` : 'No applications yet. Add your first one!'}
        </p>
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Company</th>
                <th>Role</th>
                <th>Status</th>
                <th>Applied</th>
                <th>Notes</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {applications.map((app) => (
                <tr key={app.id}>
                  <td>{app.company}</td>
                  <td>{app.role}</td>
                  <td><span className={`badge ${app.status.toLowerCase()}`}>{app.status}</span></td>
                  <td>{app.appliedDate}</td>
                  <td className="notes">{app.notes}</td>
                  <td className="row-actions">
                    <button onClick={() => openEdit(app)}>Edit</button>
                    <button className="danger" onClick={() => handleDelete(app)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
