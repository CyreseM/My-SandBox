import { useState, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as Yup from 'yup';
import { Image as ImageIcon, X } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialogs';
import { Textarea, LoadingButton, FormField, Spinner } from '@/components/ui';
import { statusApi, fileApi } from '@/lib/mutations';
import { withToast } from '@/lib/toast';
import { getConnection } from '@/lib/signalr';
import { toast } from 'sonner';

const BG_COLORS = [
  '#0ea5e9', // sky
  '#06b6d4', // cyan
  '#8b5cf6', // violet
  '#ec4899', // pink
  '#f97316', // orange
  '#22c55e', // green
  '#ef4444', // red
  '#64748b', // slate
];

const schema = Yup.object({ content: Yup.string().max(500).optional() });

export function PostStatusDialog({ onPosted, children }) {
  const [open,      setOpen]      = useState(false);
  const [mediaUrl,  setMediaUrl]  = useState(null);
  const [uploading, setUploading] = useState(false);
  const [bgColor,   setBgColor]   = useState(BG_COLORS[0]);
  const fileRef = useRef();

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } =
    useForm({ resolver: yupResolver(schema) });

  const content = watch('content');

  const handleImagePick = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const { data } = await fileApi.upload(file);
      setMediaUrl(data.url);
    } catch { toast.error('Image upload failed'); }
    finally { setUploading(false); }
  };

  const onClose = (o) => {
    setOpen(o);
    if (!o) { reset(); setMediaUrl(null); }
  };

  const onSubmit = async (values) => {
    if (!values.content?.trim() && !mediaUrl) {
      toast.error('Add some text or an image');
      return;
    }
    const { data: status } = await withToast(
      () => statusApi.post({
        content: values.content?.trim() || null,
        mediaUrl,
        mediaType: mediaUrl ? 'Image' : 'None',
        backgroundColor: bgColor,
      }),
      { loading: 'Posting status…', success: 'Status posted!' }
    );
    getConnection().invoke('BroadcastNewStatus', status.id).catch(() => {});
    onClose(false);
    onPosted?.();
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>Post a Status</DialogTitle></DialogHeader>

        {/* Preview card */}
        <div
          className="w-full h-36 rounded-xl flex items-center justify-center overflow-hidden relative mb-2"
          style={{ backgroundColor: mediaUrl ? '#000' : bgColor }}
        >
          {mediaUrl
            ? <img src={mediaUrl} alt="Preview" className="max-h-full max-w-full object-contain" />
            : content
              ? <p className="text-white text-lg font-bold text-center px-4 leading-snug">{content}</p>
              : <p className="text-white/40 text-sm">Your status preview</p>
          }
          {mediaUrl && (
            <button
              type="button"
              onClick={() => setMediaUrl(null)}
              className="absolute top-2 right-2 bg-black/50 rounded-full p-1 text-white hover:bg-black/70"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          )}
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Colour picker */}
          {!mediaUrl && (
            <div className="flex gap-2 flex-wrap">
              {BG_COLORS.map((c) => (
                <button
                  key={c} type="button" onClick={() => setBgColor(c)}
                  className="h-7 w-7 rounded-full border-2 transition-transform hover:scale-110"
                  style={{
                    backgroundColor: c,
                    borderColor: bgColor === c ? 'hsl(var(--foreground))' : 'transparent',
                    transform: bgColor === c ? 'scale(1.15)' : undefined,
                  }}
                />
              ))}
            </div>
          )}

          <FormField label="Caption" error={errors.content}>
            <Textarea {...register('content')} placeholder="What's on your mind?" rows={3} />
          </FormField>

          <div className="flex items-center gap-3">
            <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={handleImagePick} />
            <button
              type="button"
              onClick={() => fileRef.current?.click()}
              className="flex items-center justify-center h-9 w-9 rounded-lg border border-border hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
            >
              {uploading ? <Spinner size="sm" /> : <ImageIcon className="h-4 w-4" />}
            </button>
            <LoadingButton type="submit" loading={isSubmitting} className="flex-1">
              Post Status
            </LoadingButton>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
