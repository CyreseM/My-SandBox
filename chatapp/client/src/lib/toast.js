import { toast } from 'sonner';

export const toastLoading = (msg)      => toast.loading(msg);
export const toastSuccess = (id, msg)  => toast.success(msg, { id });
export const toastError   = (id, err)  => {
  const message = err?.response?.data?.message || err?.message || 'Something went wrong';
  if (id) toast.error(message, { id });
  else    toast.error(message);
};

export async function withToast(fn, { loading, success }) {
  const id = toast.loading(loading);
  try {
    const result = await fn();
    toast.success(success, { id });
    return result;
  } catch (err) {
    toastError(id, err);
    throw err;
  }
}
