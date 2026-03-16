import { useEffect } from 'react';
import { toast } from 'sonner';
import { getConnection, startConnection } from '@/lib/signalr';
import { useChatStore } from '@/store/chatStore';

export function useSignalR() {
  const store = useChatStore();

  useEffect(() => {
    const conn = getConnection();

    conn.on('ReceiveMessage', (msg) => {
      store.addMessage(msg.chatId, msg);
      if (document.visibilityState === 'hidden' && Notification.permission === 'granted') {
        new Notification(msg.senderDisplayName, {
          body: msg.content ?? '📎 Attachment',
          icon: msg.senderAvatarUrl ?? '/favicon.ico',
        });
      }
    });

    conn.on('MessageEdited',    (msg)                => store.editMessage(msg.chatId, msg));
    conn.on('MessageDeleted',   (msgId, chatId)       => store.deleteMessage(chatId, msgId));
    conn.on('ReactionsUpdated', (msgId, reactions)    => store.updateReactions(msgId, reactions));
    conn.on('UserTyping',       (chatId, uid, typing) => store.setTyping(chatId, uid, typing));
    conn.on('UserOnline',       (userId)              => store.setOnline(userId));
    conn.on('UserOffline',      (userId, lastSeen)    => store.setOffline(userId, lastSeen));
    conn.on('MessagesRead',     (chatId, uid, readAt) => store.setReadReceipt(chatId, uid, readAt));
    conn.on('NewStatus',        () => toast.info('New status from a contact', { duration: 3000 }));
    conn.on('Error',            (msg) => toast.error(msg));

    conn.onreconnected(() => toast.success('Reconnected ✓'));
    conn.onclose(()       => toast.error('Connection lost — retrying...'));

    startConnection().catch(() => toast.error('Could not connect to server'));

    return () => {
      ['ReceiveMessage','MessageEdited','MessageDeleted','ReactionsUpdated',
       'UserTyping','UserOnline','UserOffline','MessagesRead','NewStatus','Error']
        .forEach((ev) => conn.off(ev));
    };
  }, []);
}
