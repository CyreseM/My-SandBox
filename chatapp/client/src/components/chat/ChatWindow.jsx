import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useChat, useCurrentUser } from '@/hooks';
import { getConnection } from '@/lib/signalr';
import { ChatHeader } from './ChatHeader';
import { MessageList, MessageInput } from './MessageComponents';
import { FullPageLoader } from '@/components/ui';
import { MessageSquare } from 'lucide-react';

export function ChatWindow() {
  const { chatId }          = useParams();
  const { user: me }        = useCurrentUser();
  const { chat, isLoading } = useChat(chatId);
  const [replyTo, setReplyTo] = useState(null);

  useEffect(() => {
    if (chatId) {
      getConnection().invoke('MarkAsRead', chatId).catch(() => {});
    }
    setReplyTo(null);
  }, [chatId]);

  if (isLoading) return <FullPageLoader text="Loading chat…" />;
  if (!chat) return null;

  const myMembership = null; // fetched via members endpoint if needed
  const canSend = chat.type !== 'Channel'; // simplified; full role check would need members

  return (
    <div className="flex flex-col h-full">
      <ChatHeader chat={chat} myRole={null} />
      <MessageList
        chatId={chatId}
        currentUserId={me?.id}
        chatType={chat.type}
        onReply={setReplyTo}
      />
      {canSend ? (
        <MessageInput chatId={chatId} replyTo={replyTo} onClearReply={() => setReplyTo(null)} />
      ) : (
        <div className="border-t border-border p-4 text-center text-sm text-muted-foreground bg-card/50">
          Only admins can post in this channel
        </div>
      )}
    </div>
  );
}

export function EmptyState() {
  return (
    <div className="flex-1 flex flex-col items-center justify-center gap-4 text-center p-8">
      <div className="rounded-3xl bg-muted/40 p-6">
        <MessageSquare className="h-12 w-12 text-muted-foreground/50" />
      </div>
      <div>
        <h3 className="font-semibold mb-1">Start a conversation</h3>
        <p className="text-sm text-muted-foreground">Search for someone above or create a group</p>
      </div>
    </div>
  );
}
