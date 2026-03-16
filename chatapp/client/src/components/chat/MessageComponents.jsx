import { useState, useRef, useEffect, useCallback } from 'react';
import { getConnection } from '@/lib/signalr';
import { fileApi } from '@/lib/mutations';
import { useDropzone } from 'react-dropzone';
import { useChatStore } from '@/store/chatStore';
import { useMessages } from '@/hooks';
import { Button, Textarea, Spinner } from '@/components/ui';
import { MessageBubble } from './MessageBubble';
import { ReplyPreview } from './MessageBubble';
import { SendHorizonal, Smile, Paperclip } from 'lucide-react';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';

// ── FileAttachButton ──────────────────────────────────────────────────────────
function FileAttachButton({ chatId }) {
  const conn = getConnection();
  const { getRootProps, getInputProps } = useDropzone({
    noClick: false, multiple: true, maxSize: 50 * 1024 * 1024,
    onDropRejected: () => toast.error('File too large (max 50 MB)'),
    onDrop: async (files) => {
      for (const file of files) {
        const id = toast.loading(`Uploading ${file.name}…`);
        try {
          const { data: uploaded } = await fileApi.upload(file, (pct) =>
            toast.loading(`Uploading ${file.name} — ${pct}%`, { id }));
          toast.success(`${file.name} sent`, { id });
          const type = file.type.startsWith('image/') ? 'Image' : 'File';
          await conn.invoke('SendMessage', { chatId, type, content: null, attachments: [uploaded] });
        } catch { toast.error(`Failed: ${file.name}`, { id }); }
      }
    },
  });

  return (
    <div {...getRootProps()}>
      <input {...getInputProps()} />
      <Button variant="ghost" size="icon" className="h-9 w-9 shrink-0 text-muted-foreground hover:text-foreground" type="button">
        <Paperclip className="h-5 w-5" />
      </Button>
    </div>
  );
}

// ── MessageInput ──────────────────────────────────────────────────────────────
export function MessageInput({ chatId, replyTo, onClearReply }) {
  const [content, setContent] = useState('');
  const typingTimer = useRef(null);
  const conn = getConnection();

  const stopTyping = useCallback(() => {
    conn.invoke('TypingIndicator', chatId, false).catch(() => {});
  }, [chatId]);

  const handleChange = (e) => {
    setContent(e.target.value);
    conn.invoke('TypingIndicator', chatId, true).catch(() => {});
    clearTimeout(typingTimer.current);
    typingTimer.current = setTimeout(stopTyping, 2000);
  };

  const send = async () => {
    if (!content.trim()) return;
    try {
      await conn.invoke('SendMessage', {
        chatId, content: content.trim(),
        replyToId: replyTo?.id ?? null, type: 'Text',
      });
      setContent(''); onClearReply?.(); stopTyping();
    } catch { toast.error('Failed to send message'); }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); send(); }
  };

  // Quick emoji insertion
  const EMOJIS = ['😊', '👍', '❤️', '😂', '🙏', '🔥'];
  const [showEmoji, setShowEmoji] = useState(false);

  return (
    <div className="border-t border-border px-3 py-3 bg-card/80 backdrop-blur-sm shrink-0 space-y-2">
      {replyTo && <ReplyPreview message={replyTo} onClose={onClearReply} />}

      {showEmoji && (
        <div className="flex gap-1 px-1">
          {EMOJIS.map(e => (
            <button key={e} onClick={() => setContent(c => c + e)}
              className="text-xl hover:scale-125 transition-transform">{e}</button>
          ))}
        </div>
      )}

      <div className="flex items-end gap-2">
        <Button variant="ghost" size="icon" className="h-9 w-9 shrink-0 text-muted-foreground hover:text-foreground"
          onClick={() => setShowEmoji(s => !s)}>
          <Smile className="h-5 w-5" />
        </Button>
        <FileAttachButton chatId={chatId} />
        <Textarea
          value={content} onChange={handleChange} onKeyDown={handleKeyDown}
          placeholder="Message…" rows={1}
          className="min-h-[40px] max-h-40 resize-none flex-1 py-2 text-sm bg-muted/50"
        />
        <Button size="icon" className="h-9 w-9 shrink-0" onClick={send} disabled={!content.trim()}>
          <SendHorizonal className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}

// ── MessageList ───────────────────────────────────────────────────────────────
export function MessageList({ chatId, currentUserId, chatType, onReply }) {
  const { allMessages, loadMore, hasMore, isLoading } = useMessages(chatId);
  const realtimeMsgs = useChatStore((s) => s.messages[chatId] ?? []);
  const typingUsers  = useChatStore((s) => s.typingUsers[chatId] ?? {});
  const anyTyping    = Object.keys(typingUsers).length > 0;
  const bottomRef    = useRef();
  const containerRef = useRef();
  const prevLenRef   = useRef(0);

  // Combine and deduplicate messages
  const combined = (() => {
    const map = new Map();
    [...allMessages, ...realtimeMsgs].forEach(m => map.set(m.id, m));
    return Array.from(map.values()).sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt));
  })();

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    if (combined.length > prevLenRef.current) {
      const lastMsg = combined[combined.length - 1];
      if (lastMsg?.senderId === currentUserId || prevLenRef.current === 0) {
        bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
      }
    }
    prevLenRef.current = combined.length;
  }, [combined.length]);

  // Load more on scroll top
  const handleScroll = () => {
    if (containerRef.current?.scrollTop < 100 && hasMore && !isLoading) loadMore();
  };

  return (
    <div ref={containerRef} onScroll={handleScroll}
      className="flex-1 overflow-y-auto py-2 space-y-1">
      {isLoading && (
        <div className="flex justify-center py-4"><Spinner size="sm" /></div>
      )}
      {hasMore && !isLoading && (
        <div className="flex justify-center py-2">
          <button onClick={loadMore} className="text-xs text-primary hover:underline">Load more</button>
        </div>
      )}
      {combined.map((msg) => (
        <MessageBubble
          key={msg.id} message={msg}
          isMine={msg.senderId === currentUserId}
          chatType={chatType}
          onReply={onReply}
        />
      ))}
      {anyTyping && (
        <div className="px-6 py-1">
          <span className="text-xs text-muted-foreground italic animate-pulse">Someone is typing…</span>
        </div>
      )}
      <div ref={bottomRef} />
    </div>
  );
}
