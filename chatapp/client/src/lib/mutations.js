import { api } from './axios';

export const chatApi = {
  createDirect:  (userId)        => api.post('/chats/direct', { userId }),
  createGroup:   (dto)           => api.post('/chats/group', dto),
  createChannel: (dto)           => api.post('/chats/channel', dto),
  addMember:     (id, userId)    => api.post(`/chats/${id}/members`, { userId }),
  removeMember:  (id, userId)    => api.delete(`/chats/${id}/members/${userId}`),
  changeRole:    (id, uid, role) => api.put(`/chats/${id}/members/${uid}/role`, { role }),
  generateInvite:(id)            => api.post(`/chats/${id}/invite`),
  joinViaLink:   (token)         => api.get(`/chats/join/${token}`),
};

export const statusApi = {
  post:       (dto) => api.post('/statuses', dto),
  delete:     (id)  => api.delete(`/statuses/${id}`),
  markViewed: (id)  => api.post(`/statuses/${id}/view`),
  getViewers: (id)  => api.get(`/statuses/${id}/views`).then((r) => r.data),
  getById:    (id)  => api.get(`/statuses/${id}`).then((r) => r.data),
};

export const userApi = {
  updateProfile: (dto)  => api.put('/users/me', dto),
  uploadAvatar:  (file) => {
    const form = new FormData();
    form.append('avatar', file);
    return api.post('/users/me/avatar', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};

export const fileApi = {
  upload: (file, onProgress) => {
    const form = new FormData();
    form.append('file', file);
    return api.post('/files/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => onProgress?.(Math.round((e.loaded * 100) / e.total)),
    });
  },
};
