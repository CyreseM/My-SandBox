import { format } from 'date-fns';
import { CheckCheck, Reply, Copy, Trash2, FileIcon, Download } from 'lucide-react';
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator } from '@/components/ui/dialogs';
import { useChatStore } from '@/store/chatStore';
import { getConnection } from '@/lib/signalr';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';
import { useState } from 'react';
import { Dialog, DialogContent } from '@/components/ui/dialogs';
import { Avatar } from '@/components/ui';

// ── ReplyQuote ────────────────────────────────────────────────────────────────
export function ReplyQuote({ message, isMine }) {
  return (
    <div className={cn('border-l-2 pl-2 mb-2 text-xs rounded-sm py-0.5',
      isMine ? 'border-white/50 opacity-80' : 'border-primary opacity-70')}>
      <p className="font-semibold truncate">{message.senderDisplayName}</p>
      <p className="truncate text-xs opacity-80">{message.content ?? '📎 Attachment'}</p>
    </div>
  );
}

// ── ReactionBar ───────────────────────────────────────────────────────────────
export function ReactionBar({ reactions, onReact }) {
  const grouped = (reactions ?? []).reduce((acc, r) => {
    (acc[r.emoji] = acc[r.emoji] ?? []).push(r.userId); return acc;
  }, {});

  if (Object.keys(grouped).length === 0) return null;
  return (
    <div className="flex flex-wrap gap-1 mt-1.5">
      {Object.entries(grouped).map(([emoji, users]) => (
        <button key={emoji} onClick={() => onReact(emoji)}
          className="flex items-center gap-0.5 bg-background/30 hover:bg-background/50 rounded-full px-1.5 py-0.5 text-xs transition-colors">
          {emoji} <span className="opacity-80">{users.length}</span>
        </button>
      ))}
    </div>
  );
}

// ── ReadTicks ─────────────────────────────────────────────────────────────────
export function ReadTicks({ message }) {
  const readReceipts = useChatStore((s) => s.readReceipts);
  const isRead = Object.entries(readReceipts).some(([key, readAt]) => {
    const [chatId, userId] = key.split(':');
    return chatId === message.chatId &&
           userId !== message.senderId &&
           new Date(readAt) >= new Date(message.sentAt);
  });
  return <CheckCheck className={cn('h-3 w-3', isRead ? 'text-blue-400' : 'opacity-40')} />;
}

// ── ReplyPreview (in input) ───────────────────────────────────────────────────
export function ReplyPreview({ message, onClose }) {
  return (
    <div className="flex items-center gap-2 border-l-2 border-primary bg-muted/40 rounded-lg pl-3 pr-2 py-1.5">
      <div className="flex-1 min-w-0">
        <p className="text-xs font-semibold text-primary">{message.senderDisplayName}</p>
        <p className="text-xs text-muted-foreground truncate">{message.content ?? '📎 Attachment'}</p>
      </div>
      <button onClick={onClose} className="text-muted-foreground hover:text-foreground p-0.5">
        ✕
      </button>
    </div>
  );
}

// ── MediaGrid ─────────────────────────────────────────────────────────────────
function fmtBytes(b) {
  if (b < 1024) return `${b} B`;
  if (b < 1024 ** 2) return `${(b / 1024).toFixed(1)} KB`;
  return `${(b / 1024 ** 2).toFixed(1)} MB`;
}

export function MediaGrid({ attachments }) {
  const [lightbox, setLightbox] = useState(null);
  const images = (attachments ?? []).filter(a => a.mimeType?.startsWith('image/'));
  const files  = (attachments ?? []).filter(a => !a.mimeType?.startsWith('image/'));

  return (
    <div className="space-y-1 mt-1">
      {images.length > 0 && (
        <div className={cn('grid gap-1', images.length > 1 ? 'grid-cols-2' : 'grid-cols-1')}>
          {images.map((img) => (
            <div key={img.id} className="relative">
              <img src={img.url} alt={img.fileName}
                className="rounded-lg object-cover cursor-zoom-in max-h-48 w-full"
                onClick={() => setLightbox(img)} />
              {/* download icon on each thumbnail */}
              <a href={img.url} download={img.fileName}
                className="absolute bottom-1 right-1 bg-background/30 hover:bg-background/50 rounded-full p-1">
                <Download className="h-4 w-4" />
              </a>
            </div>
          ))}
        </div>
      )}
      {files.map((file) => (
        <a key={file.id} href={file.url} download={file.fileName}
          className="flex items-center gap-2 bg-background/20 hover:bg-background/30 rounded-lg px-3 py-2 transition-colors">
          <FileIcon className="h-5 w-5 shrink-0" />
          <div className="flex-1 min-w-0">
            <p className="text-xs font-medium truncate">{file.fileName}</p>
            <p className="text-[10px] opacity-60">{fmtBytes(file.fileSize)}</p>
          </div>
          <Download className="h-4 w-4 shrink-0 opacity-60" />
        </a>
      ))}
      <Dialog open={!!lightbox} onOpenChange={() => setLightbox(null)}>
        <DialogContent className="relative max-w-4xl p-1 bg-black border-none">
          {lightbox && (
            <>
              <img src={lightbox.url} alt={lightbox.fileName} className="max-h-[90vh] w-full object-contain rounded-lg" />
              <a href={lightbox.url} download={lightbox.fileName}
                className="absolute top-2 right-2 bg-background/30 hover:bg-background/50 rounded-full p-1">
                <Download className="h-5 w-5 text-white" />
              </a>
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

// ── MessageBubble ─────────────────────────────────────────────────────────────
export function MessageBubble({ message, isMine, chatType, onReply }) {
  const conn = getConnection();

  if (message.isDeleted) return (
    <div className={cn('flex px-4 py-1', isMine && 'justify-end')}>
      <span className="text-xs italic text-muted-foreground bg-muted/30 rounded-full px-3 py-1">
        Message deleted
      </span>
    </div>
  );

  const react = (emoji) => conn.invoke('ReactToMessage', message.id, emoji);
  const copy  = () => { navigator.clipboard.writeText(message.content ?? ''); toast.success('Copied!'); };
  const del   = () => conn.invoke('DeleteMessage', message.id);

  const QUICK_EMOJIS = ['👍', '❤️', '😂', '😮', '😢', '🙏'];

  return (
    <div className={cn('flex gap-2 px-4 py-1 group animate-fade-in', isMine ? 'flex-row-reverse' : '')}>
      {!isMine && chatType !== 'Direct' && (
        <Avatar src={message.senderAvatarUrl} fallback={message.senderDisplayName} size="sm"
          className="self-end mb-1 shrink-0" />
      )}

      <div className="max-w-[72%]">
        {!isMine && chatType !== 'Direct' && (
          <p className="text-xs font-semibold text-primary mb-1 ml-1">{message.senderDisplayName}</p>
        )}

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <div className={cn(
              'rounded-2xl px-3.5 py-2.5 text-sm cursor-pointer select-text',
              isMine ? 'bubble-mine' : 'bubble-theirs'
            )}>
              {message.replyTo && <ReplyQuote message={message.replyTo} isMine={isMine} />}
              {message.attachments?.length > 0 && <MediaGrid attachments={message.attachments} />}
              {message.content && <p className="whitespace-pre-wrap break-words leading-relaxed">{message.content}</p>}

              <div className={cn('flex items-center gap-1 mt-1.5', isMine ? 'justify-end' : 'justify-start')}>
                <span className="text-[10px] opacity-55">{format(new Date(message.sentAt), 'HH:mm')}</span>
                {message.isEdited && <span className="text-[10px] opacity-40">edited</span>}
                {isMine && <ReadTicks message={message} />}
              </div>

              <ReactionBar reactions={message.reactions} onReact={react} />
            </div>
          </DropdownMenuTrigger>

          <DropdownMenuContent align={isMine ? 'end' : 'start'}>
            <div className="flex gap-1.5 px-2 py-1.5 border-b border-border">
              {QUICK_EMOJIS.map((e) => (
                <button key={e} onClick={() => react(e)}
                  className="text-lg hover:scale-125 transition-transform leading-none">{e}</button>
              ))}
            </div>
            <DropdownMenuItem onSelect={() => onReply?.(message)}>
              <Reply className="h-4 w-4 mr-2" />Reply
            </DropdownMenuItem>
            {message.content && (
              <DropdownMenuItem onSelect={copy}>
                <Copy className="h-4 w-4 mr-2" />Copy Text
              </DropdownMenuItem>
            )}
            {isMine && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem onSelect={del} className="text-destructive">
                  <Trash2 className="h-4 w-4 mr-2" />Delete
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  );
}
