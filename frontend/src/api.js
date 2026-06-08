const BASE_URL = 'http://localhost:5015/api'

export function getToken() {
  return localStorage.getItem('token')
}

export function getProfessor() {
  const raw = localStorage.getItem('professor')
  return raw ? JSON.parse(raw) : null
}

export function setAuth(data) {
  localStorage.setItem('token', data.token)
  localStorage.setItem('professor', JSON.stringify({ id: data.id, name: data.name, email: data.email }))
}

export function clearAuth() {
  localStorage.removeItem('token')
  localStorage.removeItem('professor')
}

async function request(path, { method = 'GET', body, isForm = false } = {}) {
  const headers = {}
  const token = getToken()
  if (token) headers['Authorization'] = `Bearer ${token}`

  let payload = body
  if (body && !isForm) {
    headers['Content-Type'] = 'application/json'
    payload = JSON.stringify(body)
  }

  const res = await fetch(`${BASE_URL}${path}`, { method, headers, body: payload })

  if (res.status === 401) {
    clearAuth()
    window.location.href = '/'
    throw new Error('Session expired. Please log in again.')
  }

  if (!res.ok) {
    let message = `Request failed (${res.status})`
    try {
      const data = await res.json()
      if (data && data.message) message = data.message
    } catch {
      // response had no JSON body
    }
    throw new Error(message)
  }

  if (res.status === 204) return null
  const text = await res.text()
  return text ? JSON.parse(text) : null
}

export const login = (email, password) =>
  request('/auth/login', { method: 'POST', body: { email, password } })

export const register = (name, email, password) =>
  request('/auth/register', { method: 'POST', body: { name, email, password } })

export const getStudents = () => request('/students')
export const getStudent = (id) => request(`/students/${id}`)
export const createStudent = (data) => request('/students', { method: 'POST', body: data })
export const updateStudent = (id, data) => request(`/students/${id}`, { method: 'PUT', body: data })
export const deleteStudent = (id) => request(`/students/${id}`, { method: 'DELETE' })

export const getSessions = (studentId) => request(`/sessions?studentId=${studentId}`)
export const createSession = (formData) => request('/sessions', { method: 'POST', body: formData, isForm: true })
export const updateSession = (id, formData) => request(`/sessions/${id}`, { method: 'PUT', body: formData, isForm: true })
export const deleteSession = (id) => request(`/sessions/${id}`, { method: 'DELETE' })

export async function downloadSessionFile(id, fileName) {
  const res = await fetch(`${BASE_URL}/sessions/${id}/file`, {
    headers: { Authorization: `Bearer ${getToken()}` }
  })
  if (!res.ok) throw new Error('Could not download file')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName || 'download'
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}
