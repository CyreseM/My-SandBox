import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/dialogs';
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem } from '@/components/ui/dialogs';
import { Button, Badge, Spinner } from '@/components/ui';
import { OnlineAvatar } from '@/components/shared/OnlineAvatar';
import { chatApi } from '@/lib/mutations';
import { withToast } from '@/lib/toast';
import { useMembers } from '@/hooks';
import { MoreVertical, UserMinus, ShieldCheck, Link2 } from 'lucide-react';
import { toast } from 'sonner';

export function GroupInfoSheet({ chat, open, onClose, currentUserRole }) {
  const { members, isLoading, mutate } = useMembers(chat?.id, open);
  const canManage = ['Admin', 'Owner'].includes(currentUserRole);

  const removeMember = (uid) =>
    withToast(() => chatApi.removeMember(chat.id, uid).then(mutate),
              { loading: 'Removing…', success: 'Member removed' });

  const promoteAdmin = (uid) =>
    withToast(() => chatApi.changeRole(chat.id, uid, 'Admin').then(mutate),
              { loading: 'Updating role…', success: 'Promoted to admin' });

  const copyLink = async () => {
    const { data } = await withToast(
      () => chatApi.generateInvite(chat.id),
      { loading: 'Generating link…', success: 'Invite link copied!' }
    );
    navigator.clipboard.writeText(data.link);
  };

  if (!chat) return null;

  return (
    <Sheet open={open} onOpenChange={onClose}>
      <SheetContent>
        <SheetHeader>
          <SheetTitle>{chat.name}</SheetTitle>
        </SheetHeader>
        <div className="px-6 py-4 space-y-5">
          {chat.description && (
            <p className="text-sm text-muted-foreground">{chat.description}</p>
          )}

          {canManage && (
            <Button variant="outline" size="sm" onClick={copyLink} className="w-full gap-2">
              <Link2 className="h-4 w-4" /> Copy Invite Link
            </Button>
          )}

          <div>
            <h4 className="text-sm font-semibold mb-3">Members ({isLoading ? '…' : members.length})</h4>
            {isLoading && <div className="flex justify-center py-4"><Spinner size="sm" /></div>}
            <div className="space-y-1">
              {members.map((m) => (
                <div key={m.userId} className="flex items-center gap-3 p-2 rounded-xl hover:bg-muted/50">
                  <OnlineAvatar user={m.user} size="sm" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{m.user.displayName}</p>
                    <p className="text-xs text-muted-foreground">@{m.user.username}</p>
                  </div>
                  {m.role !== 'Member' && (
                    <Badge variant="secondary" className="text-[10px]">{m.role}</Badge>
                  )}
                  {canManage && m.role === 'Member' && (
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon" className="h-7 w-7">
                          <MoreVertical className="h-3.5 w-3.5" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onSelect={() => promoteAdmin(m.userId)}>
                          <ShieldCheck className="h-4 w-4 mr-2" />Make Admin
                        </DropdownMenuItem>
                        <DropdownMenuItem onSelect={() => removeMember(m.userId)} className="text-destructive">
                          <UserMinus className="h-4 w-4 mr-2" />Remove
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
