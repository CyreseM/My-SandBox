import { api } from './axios'

export const searchUsersApi = async (query) => {
  const { data } = await api.get(`/api/users/search?q=${encodeURIComponent(query)}`)
  return data
}

export const getMeApi = async () => {
  const { data } = await api.get('/api/users/me')
  return data
}
