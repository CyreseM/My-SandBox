// fetcher.js
import { api } from './axios';
export const fetcher = (url) => api.get(url).then((r) => r.data);
