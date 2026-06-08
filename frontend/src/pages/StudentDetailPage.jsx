import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  getStudent, getSessions, getStudentStats, createSession, updateSession,
  deleteSession, downloadSessionFile
} from '../api'

const emptyForm = { title: '', description: '', date: '', time: '', file: null, removeFile: false }

function pad(n) {
  return String(n).padStart(2, '0')
}

function toFormDate(iso) {
  const d = new Date(iso)
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

function toFormTime(iso) {
  const d = new Date(iso)
  return `${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function parseFormDateTime(dateStr, timeStr) {
  const dm = dateStr.trim().match(/^(\d{2})\/(\d{2})\/(\d{4})$/)
  if (!dm) return null
  const [, dd, mm, yyyy] = dm
  const tm = (timeStr || '').trim().match(/^(\d{1,2}):(\d{2})$/)
  const hours = tm ? Number(tm[1]) : 0
  const minutes = tm ? Number(tm[2]) : 0
  const d = new Date(Number(yyyy), Number(mm) - 1, Number(dd), hours, minutes)
  return Number.isNaN(d.getTime()) ? null : d
}

function formatDate(iso) {
  if (!iso) return '—'
  const d = new Date(iso)
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

function formatDateTime(iso) {
  if (!iso) return '—'
  const d = new Date(iso)
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function relativeDays(days) {
  if (days === null || days === undefined) return '—'
  if (days <= 0) return 'Today'
  if (days === 1) return 'Yesterday'
  return `${days} days ago`
}

function TrendChart({ data }) {
  const max = Math.max(1, ...data.map((d) => d.count))
  const W = 480
  const H = 170
  const padX = 20
  const padTop = 24
  const padBottom = 28
  const barW = (W - padX * 2) / data.length

  return (
    <svg viewBox={`0 0 ${W} ${H}`} className="trend" role="img" aria-label="Sessions per month">
      {data.map((d, i) => {
        const h = (d.count / max) * (H - padTop - padBottom)
        const x = padX + i * barW
        const y = H - padBottom - h
        return (
          <g key={`${d.year}-${d.month}`}>
            <rect x={x + 6} y={y} width={barW - 12} height={h} fill="#0366d6" rx="3" />
            {d.count > 0 && (
              <text x={x + barW / 2} y={y - 5} textAnchor="middle" fontSize="11" fill="#222">{d.count}</text>
            )}
            <text x={x + barW / 2} y={H - 8} textAnchor="middle" fontSize="11" fill="#777">{d.label}</text>
          </g>
        )
      })}
    </svg>
  )
}

function Stat({ value, label, sub }) {
  return (
    <div className="stat">
      <div className="value">{value}</div>
      <div className="label">{label}</div>
      {sub && <div className="sub">{sub}</div>}
    </div>
  )
}

export default function StudentDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [student, setStudent] = useState(null)
  const [sessions, setSessions] = useState([])
  const [stats, setStats] = useState(null)
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(emptyForm)

  async function load() {
    setError('')
    try {
      const [st, ss, stat] = await Promise.all([getStudent(id), getSessions(id), getStudentStats(id)])
      setStudent(st)
      setSessions(ss)
      setStats(stat)
    } catch (err) {
      setError(err.message)
    }
  }

  useEffect(() => { load() }, [id])

  function startCreate() {
    setEditingId(null)
    setForm(emptyForm)
    setShowForm(true)
  }

  function startEdit(s) {
    setEditingId(s.id)
    setForm({
      title: s.title,
      description: s.description || '',
      date: s.date ? toFormDate(s.date) : '',
      time: s.date ? toFormTime(s.date) : '',
      file: null,
      removeFile: false
    })
    setShowForm(true)
  }

  function cancelForm() {
    setShowForm(false)
    setEditingId(null)
    setForm(emptyForm)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    const when = parseFormDateTime(form.date, form.time)
    if (!when) {
      setError('Enter the date as dd/mm/yyyy and a valid time.')
      return
    }
    try {
      const fd = new FormData()
      fd.append('Title', form.title)
      fd.append('Description', form.description || '')
      fd.append('Date', when.toISOString())
      if (form.file) fd.append('File', form.file)
      if (editingId) {
        fd.append('RemoveFile', form.removeFile ? 'true' : 'false')
        await updateSession(editingId, fd)
      } else {
        fd.append('StudentId', id)
        await createSession(fd)
      }
      cancelForm()
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(sid) {
    if (!confirm('Delete this session?')) return
    setError('')
    try {
      await deleteSession(sid)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDownload(s) {
    setError('')
    try {
      await downloadSessionFile(s.id, s.fileName)
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="container">
      <button className="link" onClick={() => navigate('/dashboard')}>&larr; Back to students</button>
      <h1>{student ? student.name : 'Student'}</h1>
      {student && (
        <p className="muted">{student.email}{student.subject ? ` · ${student.subject}` : ''}</p>
      )}

      {error && <p className="error">{error}</p>}

      <h2>Statistics</h2>
      {!stats || stats.total === 0 ? (
        <p className="muted">No data yet. Add a session to see statistics.</p>
      ) : (
        <>
          <div className="stat-grid">
            <Stat value={stats.total} label="Total sessions" />
            <Stat value={stats.upcomingCount} label="Upcoming" sub={stats.nextSession ? `next ${formatDate(stats.nextSession)}` : 'none scheduled'} />
            <Stat value={relativeDays(stats.daysSinceLast)} label="Last session" sub={stats.lastSession ? formatDate(stats.lastSession) : undefined} />
            <Stat value={formatDate(stats.firstSession)} label="First session" />
            <Stat value={stats.withFile} label="With materials" sub={`${stats.withFilePercent}% of sessions`} />
          </div>
          <TrendChart data={stats.months} />
        </>
      )}

      <h2>Sessions</h2>
      <button onClick={startCreate}>Add session</button>

      {showForm && (
        <form onSubmit={handleSubmit} className="card">
          <h3>{editingId ? 'Edit session' : 'New session'}</h3>
          <label>Title
            <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required />
          </label>
          <label>Description
            <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          </label>
          <label>Date
            <input
              type="text"
              inputMode="numeric"
              placeholder="dd/mm/yyyy"
              pattern="\d{2}/\d{2}/\d{4}"
              value={form.date}
              onChange={(e) => setForm({ ...form, date: e.target.value })}
              required
            />
          </label>
          <label>Time
            <input type="time" value={form.time} onChange={(e) => setForm({ ...form, time: e.target.value })} required />
          </label>
          <label>File
            <input type="file" onChange={(e) => setForm({ ...form, file: e.target.files[0] || null })} />
          </label>
          {editingId && (
            <label className="checkbox">
              <input type="checkbox" checked={form.removeFile} onChange={(e) => setForm({ ...form, removeFile: e.target.checked })} />
              Remove existing file
            </label>
          )}
          <div className="row">
            <button type="submit">{editingId ? 'Save' : 'Create'}</button>
            <button type="button" onClick={cancelForm}>Cancel</button>
          </div>
        </form>
      )}

      {sessions.length === 0 ? (
        <p className="muted">No sessions with this student yet.</p>
      ) : (
        <table>
          <thead>
            <tr><th>Title</th><th>Date</th><th>Description</th><th>File</th><th>Reminder</th><th></th></tr>
          </thead>
          <tbody>
            {sessions.map((s) => (
              <tr key={s.id}>
                <td>{s.title}</td>
                <td>{formatDateTime(s.date)}</td>
                <td>{s.description || '-'}</td>
                <td>{s.hasFile ? <button className="link" onClick={() => handleDownload(s)}>{s.fileName}</button> : '-'}</td>
                <td>{s.sentReminder ? 'Sent' : 'Pending'}</td>
                <td className="row">
                  <button onClick={() => startEdit(s)}>Edit</button>
                  <button onClick={() => handleDelete(s.id)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
