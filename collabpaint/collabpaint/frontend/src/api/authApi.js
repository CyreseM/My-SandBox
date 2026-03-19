import { api } from './axios'

export const loginApi = async (email, password) => {
  const { data } = await api.post('/api/auth/login', { email, password })
  return data
}

export const registerApi = async (username, email, password, displayName) => {
  const { data } = await api.post('/api/auth/register', { username, email, password, displayName })
  return data
}
