import { useState } from 'react';
import { formatDistanceToNow } from 'date-fns';
import { Info } from 'lucide-react';
import { Avatar, Button } from '@/components/ui';
import { GroupInfoSheet } from './GroupInfoSheet';
import { useChatStore } from '@/store/chatStore';

export function ChatHeader({ chat, myRole }) {
  const [infoOpen, setInfoOpen] = useState(false);
  const onlineUsers = useChatStore((s) => s.onlineUsers);
  const lastSeenMap = useChatStore((s) => s.lastSeenMap);

  const isDirect = chat.type === 'Direct';
  const otherId  = isDirect ? chat.otherUserId : null;
  const isOnline = otherId && onlineUsers.has(otherId);
  const lastSeen = otherId && lastSeenMap[otherId];

  const subtitle = isDirect
    ? isOnline
      ? 'online now'
      : lastSeen
        ? `last seen ${formatDistanceToNow(new Date(lastSeen), { addSuffix: true })}`
        : 'offline'
    : chat.type === 'Group'
      ? `${chat.memberCount ?? 0} members`
      : `${chat.memberCount ?? 0} subscribers`;

  return (
    <>
      <header className="flex items-center gap-3 px-4 py-3 border-b border-border bg-card/80 backdrop-blur-sm shrink-0">
        <Avatar src={chat.avatarUrl} fallback={chat.name ?? 'D'} size="md" />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold truncate">{chat.name ?? 'Direct Message'}</p>
          <p className={`text-xs truncate ${isOnline ? 'text-green-400' : 'text-muted-foreground'}`}>
            {subtitle}
          </p>
        </div>
        {!isDirect && (
          <Button variant="ghost" size="icon" onClick={() => setInfoOpen(true)}>
            <Info className="h-4 w-4" />
          </Button>
        )}
      </header>
      {!isDirect && (
        <GroupInfoSheet
          chat={chat} open={infoOpen} onClose={() => setInfoOpen(false)} currentUserRole={myRole}
        />
      )}
    </>
  );
}
