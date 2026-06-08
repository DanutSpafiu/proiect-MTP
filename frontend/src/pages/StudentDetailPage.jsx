import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  getStudent, getSessions, createSession, updateSession,
  deleteSession, downloadSessionFile
} from '../api'

const emptyForm = { title: '', description: '', date: '', file: null, removeFile: false }

function toInputDate(iso) {
  const d = new Date(iso)
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

export default function StudentDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [student, setStudent] = useState(null)
  const [sessions, setSessions] = useState([])
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(emptyForm)

  async function load() {
    setError('')
    try {
      const [st, ss] = await Promise.all([getStudent(id), getSessions(id)])
      setStudent(st)
      setSessions(ss)
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
      date: s.date ? toInputDate(s.date) : '',
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
    try {
      const fd = new FormData()
      fd.append('Title', form.title)
      fd.append('Description', form.description || '')
      fd.append('Date', new Date(form.date).toISOString())
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
          <label>Date &amp; time
            <input type="datetime-local" value={form.date} onChange={(e) => setForm({ ...form, date: e.target.value })} required />
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
                <td>{new Date(s.date).toLocaleString()}</td>
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
