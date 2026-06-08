import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  getStudents, createStudent, updateStudent, deleteStudent,
  getProfessor, clearAuth
} from '../api'

const emptyForm = { name: '', email: '', phone: '', subject: '' }

export default function DashboardPage() {
  const navigate = useNavigate()
  const professor = getProfessor()
  const [students, setStudents] = useState([])
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(emptyForm)

  async function load() {
    setError('')
    try {
      setStudents(await getStudents())
    } catch (err) {
      setError(err.message)
    }
  }

  useEffect(() => { load() }, [])

  function startCreate() {
    setEditingId(null)
    setForm(emptyForm)
    setShowForm(true)
  }

  function startEdit(student) {
    setEditingId(student.id)
    setForm({
      name: student.name,
      email: student.email,
      phone: student.phone || '',
      subject: student.subject || ''
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
      const payload = {
        name: form.name,
        email: form.email,
        phone: form.phone || null,
        subject: form.subject || null
      }
      if (editingId) await updateStudent(editingId, payload)
      else await createStudent(payload)
      cancelForm()
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this student and all their sessions?')) return
    setError('')
    try {
      await deleteStudent(id)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  function logout() {
    clearAuth()
    navigate('/')
  }

  return (
    <div className="container">
      <header className="topbar">
        <h1>Students</h1>
        <div className="row">
          <span className="muted">{professor && professor.name}</span>
          <button onClick={logout}>Logout</button>
        </div>
      </header>

      {error && <p className="error">{error}</p>}

      <button onClick={startCreate}>Create</button>

      {showForm && (
        <form onSubmit={handleSubmit} className="card">
          <h3>{editingId ? 'Edit student' : 'New student'}</h3>
          <label>Name
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          </label>
          <label>Email
            <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
          </label>
          <label>Phone
            <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
          </label>
          <label>Subject
            <input value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} />
          </label>
          <div className="row">
            <button type="submit">{editingId ? 'Save' : 'Create'}</button>
            <button type="button" onClick={cancelForm}>Cancel</button>
          </div>
        </form>
      )}

      {students.length === 0 ? (
        <p className="muted">No students yet. Click "Create" to add one.</p>
      ) : (
        <table>
          <thead>
            <tr><th>Name</th><th>Email</th><th>Phone</th><th>Subject</th><th></th></tr>
          </thead>
          <tbody>
            {students.map((s) => (
              <tr key={s.id}>
                <td><button className="link" onClick={() => navigate(`/students/${s.id}`)}>{s.name}</button></td>
                <td>{s.email}</td>
                <td>{s.phone || '-'}</td>
                <td>{s.subject || '-'}</td>
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
