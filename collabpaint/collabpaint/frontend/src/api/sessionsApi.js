import { api } from './axios'

export const createSessionApi  = async (name)    => { const { data } = await api.post('/api/sessions', { name }); return data }
export const getSessionsApi    = async ()         => { const { data } = await api.get('/api/sessions');            return data }
export const getSessionApi     = async (id)       => { const { data } = await api.get(`/api/sessions/${id}`);     return data }
export const deleteSessionApi  = async (id)       => api.delete(`/api/sessions/${id}`)
export const getStrokesApi     = async (id)       => { const { data } = await api.get(`/api/sessions/${id}/strokes`); return data }
export const saveSnapshotApi   = async (id, snap) => api.post(`/api/sessions/${id}/snapshot`, { snapshotBase64: snap })
