import axios from 'axios'

const BASE = import.meta.env.VITE_API_BASE_URL || ''

export const api = axios.create({
  baseURL: BASE,
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  try {
    const raw = localStorage.getItem('collabpaint-auth')
    if (raw) {
      const token = JSON.parse(raw)?.state?.token
      if (token) config.headers.Authorization = `Bearer ${token}`
    }
  } catch { /* ignore */ }
  return config
})
