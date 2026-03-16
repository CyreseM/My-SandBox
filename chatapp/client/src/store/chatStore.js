import { create } from 'zustand';

export const useChatStore = create((set) => ({
  messages:     {},   // chatId -> Message[]
  typingUsers:  {},   // chatId -> { userId: bool }
  onlineUsers:  new Set(),
  lastSeenMap:  {},   // userId -> ISO string
  readReceipts: {},   // "chatId:userId" -> ISO string

  addMessage: (chatId, msg) =>
    set((s) => ({
      messages: {
        ...s.messages,
        [chatId]: [...(s.messages[chatId] ?? []).filter(m => m.id !== msg.id), msg]
      }
    })),

  setMessages: (chatId, msgs) =>
    set((s) => ({ messages: { ...s.messages, [chatId]: msgs } })),

  editMessage: (chatId, updated) =>
    set((s) => ({
      messages: {
        ...s.messages,
        [chatId]: (s.messages[chatId] ?? []).map((m) => m.id === updated.id ? updated : m)
      }
    })),

  deleteMessage: (chatId, msgId) =>
    set((s) => ({
      messages: {
        ...s.messages,
        [chatId]: (s.messages[chatId] ?? []).map((m) =>
          m.id === msgId ? { ...m, isDeleted: true, content: null } : m)
      }
    })),

  updateReactions: (msgId, reactions) =>
    set((s) => {
      const updated = {};
      for (const [cid, msgs] of Object.entries(s.messages))
        updated[cid] = msgs.map((m) => m.id === msgId ? { ...m, reactions } : m);
      return { messages: updated };
    }),

  setOnline:  (userId) =>
    set((s) => { const n = new Set(s.onlineUsers); n.add(userId); return { onlineUsers: n }; }),

  setOffline: (userId, lastSeen) =>
    set((s) => {
      const n = new Set(s.onlineUsers); n.delete(userId);
      return { onlineUsers: n, lastSeenMap: { ...s.lastSeenMap, [userId]: lastSeen } };
    }),

  setTyping: (chatId, userId, isTyping) =>
    set((s) => {
      const cur = { ...(s.typingUsers[chatId] ?? {}) };
      if (isTyping) cur[userId] = true; else delete cur[userId];
      return { typingUsers: { ...s.typingUsers, [chatId]: cur } };
    }),

  setReadReceipt: (chatId, userId, readAt) =>
    set((s) => ({
      readReceipts: { ...s.readReceipts, [`${chatId}:${userId}`]: readAt }
    })),
}));
