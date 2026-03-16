import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as Yup from 'yup';
import { Dialog, DialogContent, DialogHeader, DialogTrigger, DialogTitle } from '@/components/ui/dialogs';
import { Input, Textarea, LoadingButton, FormField, Switch } from '@/components/ui';
import { MemberPicker } from '@/components/shared/MemberPicker';
import { chatApi } from '@/lib/mutations';
import { withToast } from '@/lib/toast';
import { getConnection } from '@/lib/signalr';
import { useCurrentUser } from '@/hooks';

// ── Create Group ──────────────────────────────────────────────────────────────
const groupSchema = Yup.object({
  name:        Yup.string().min(2, 'Min 2 chars').max(128).required('Group name required'),
  description: Yup.string().max(255).optional(),
  memberIds:   Yup.array(Yup.string()).min(1, 'Add at least one member').required(),
});

export function CreateGroupDialog({ children, onCreated }) {
  const [open, setOpen]       = useState(false);
  const [selectedIds, setIds] = useState([]);
  const { user: me }          = useCurrentUser();
  const navigate              = useNavigate();

  const { register, handleSubmit, setValue, formState: { errors, isSubmitting } } =
    useForm({ resolver: yupResolver(groupSchema), defaultValues: { memberIds: [] } });

  const toggle = (uid) => {
    const next = selectedIds.includes(uid) ? selectedIds.filter(i => i !== uid) : [...selectedIds, uid];
    setIds(next);
    setValue('memberIds', next, { shouldValidate: true });
  };

  const onSubmit = async (values) => {
    const { data: chat } = await withToast(
      () => chatApi.createGroup(values),
      { loading: 'Creating group…', success: `"${values.name}" created!` }
    );
    await getConnection().invoke('JoinChatGroup', chat.id);
    setOpen(false); setIds([]); onCreated?.();
    navigate(`/chat/${chat.id}`);
  };

  return (
    <Dialog open={open} onOpenChange={(o) => { setOpen(o); if (!o) setIds([]); }}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent>
        <DialogHeader><DialogTitle>New Group</DialogTitle></DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <FormField label="Group Name" error={errors.name}>
            <Input {...register('name')} placeholder="Team Alpha" autoFocus />
          </FormField>
          <FormField label="Description" error={errors.description}>
            <Textarea {...register('description')} placeholder="What's this group about?" rows={2} />
          </FormField>
          <FormField label="Add Members" error={errors.memberIds}>
            <MemberPicker selectedIds={selectedIds} onToggle={toggle} excludeIds={me?.id ? [me.id] : []} />
          </FormField>
          <LoadingButton type="submit" loading={isSubmitting} className="w-full">Create Group</LoadingButton>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ── Create Channel ────────────────────────────────────────────────────────────
const channelSchema = Yup.object({
  name:        Yup.string().min(2, 'Min 2 chars').max(128).required('Channel name required'),
  description: Yup.string().max(500).optional(),
  isPublic:    Yup.boolean().required(),
});

export function CreateChannelDialog({ children, onCreated }) {
  const [open, setOpen] = useState(false);
  const navigate        = useNavigate();

  const { register, handleSubmit, setValue, watch, formState: { errors, isSubmitting } } =
    useForm({ resolver: yupResolver(channelSchema), defaultValues: { isPublic: true } });

  const isPublic = watch('isPublic');

  const onSubmit = async (values) => {
    const { data: chat } = await withToast(
      () => chatApi.createChannel(values),
      { loading: 'Creating channel…', success: `"${values.name}" created!` }
    );
    await getConnection().invoke('JoinChatGroup', chat.id);
    setOpen(false); onCreated?.();
    navigate(`/chat/${chat.id}`);
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent>
        <DialogHeader><DialogTitle>New Channel</DialogTitle></DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <FormField label="Channel Name" error={errors.name}>
            <Input {...register('name')} placeholder="announcements" autoFocus />
          </FormField>
          <FormField label="Description" error={errors.description}>
            <Textarea {...register('description')} placeholder="What is this channel for?" rows={2} />
          </FormField>
          <div className="flex items-center justify-between rounded-xl border border-border p-3 bg-muted/20">
            <div>
              <p className="text-sm font-medium">Public Channel</p>
              <p className="text-xs text-muted-foreground">
                {isPublic ? 'Anyone with link can join' : 'Invite-only access'}
              </p>
            </div>
            <Switch checked={isPublic} onCheckedChange={(v) => setValue('isPublic', v)} />
          </div>
          <LoadingButton type="submit" loading={isSubmitting} className="w-full">Create Channel</LoadingButton>
        </form>
      </DialogContent>
    </Dialog>
  );
}
