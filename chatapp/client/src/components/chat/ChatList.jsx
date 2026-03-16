import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';
import { Hash, Users, MessageSquare } from 'lucide-react';
import { Avatar, Skeleton } from '@/components/ui';
import { useChatStore } from '@/store/chatStore';
import { useChats } from '@/hooks';
import { cn } from '@/lib/utils';

function ChatIcon({ type }) {
  if (type === 'Channel') return <Hash className="h-4 w-4 text-muted-foreground" />;
  if (type === 'Group')   return <Users className="h-4 w-4 text-muted-foreground" />;
  return null;
}

function ChatListSkeleton() {
  return (
    <div className="space-y-1 p-2">
      {Array.from({ length: 7 }).map((_, i) => (
        <div key={i} className="flex items-center gap-3 px-3 py-2.5">
          <Skeleton className="h-11 w-11 rounded-full shrink-0" />
          <div className="flex-1 space-y-1.5">
            <Skeleton className="h-3 w-3/4" />
            <Skeleton className="h-3 w-1/2" />
          </div>
        </div>
      ))}
    </div>
  );
}

function ChatListItem({ chat, active }) {
  const navigate     = useNavigate();
  const realtimeMsgs = useChatStore((s) => s.messages[chat.id] ?? []);
  const lastMsg      = realtimeMsgs[realtimeMsgs.length - 1] ?? chat.lastMessage;

  const name = chat.name ?? 'Direct Message';
  const preview = lastMsg?.isDeleted
    ? 'Message deleted'
    : lastMsg?.content ?? (lastMsg ? '📎 Attachment' : '');

  return (
    <button
      onClick={() => navigate(`/chat/${chat.id}`)}
      className={cn(
        'w-full flex items-center gap-3 px-3 py-2.5 text-left transition-all duration-150 rounded-xl mx-1',
        active
          ? 'bg-primary/10 border border-primary/20 text-foreground'
          : 'hover:bg-muted/60 text-foreground'
      )}
    >
      <div className="relative shrink-0">
        <Avatar src={chat.avatarUrl} fallback={name} size="md" />
        {chat.type !== 'Direct' && (
          <div className="absolute -bottom-0.5 -right-0.5 bg-card border border-border rounded-full p-0.5">
            <ChatIcon type={chat.type} />
          </div>
        )}
      </div>

      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between mb-0.5">
          <span className={cn('text-sm truncate', active ? 'font-semibold' : 'font-medium')}>{name}</span>
          {lastMsg && (
            <span className="text-[10px] text-muted-foreground ml-1 shrink-0">
              {formatDistanceToNow(new Date(lastMsg.sentAt), { addSuffix: false })}
            </span>
          )}
        </div>
        <div className="flex items-center justify-between">
          <p className={cn('text-xs truncate', lastMsg?.isDeleted ? 'italic text-muted-foreground' : 'text-muted-foreground')}>
            {preview || 'No messages yet'}
          </p>
          {(chat.unreadCount ?? 0) > 0 && (
            <span className="ml-1 shrink-0 min-w-[18px] h-[18px] flex items-center justify-center rounded-full bg-primary text-primary-foreground text-[10px] font-bold px-1">
              {(chat.unreadCount ?? 0) > 99 ? '99+' : chat.unreadCount}
            </span>
          )}
        </div>
      </div>
    </button>
  );
}

export function ChatList() {
  const { chats, isLoading } = useChats();
  const { chatId: active }   = useParams();

  if (isLoading) return <ChatListSkeleton />;

  return (
    <div className="flex-1 overflow-y-auto py-2 space-y-0.5">
      {chats.length === 0 && (
        <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
          <MessageSquare className="h-10 w-10 text-muted-foreground/40 mb-3" />
          <p className="text-sm text-muted-foreground">Search above to start chatting</p>
        </div>
      )}
      {chats.map((chat) => (
        <ChatListItem key={chat.id} chat={chat} active={chat.id === active} />
      ))}
    </div>
  );
}
