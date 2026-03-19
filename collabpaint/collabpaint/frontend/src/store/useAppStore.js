import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const useAppStore = create(
  persist(
    (set) => ({
      // ── Auth (persisted) ──────────────────────────────
      token:       null,
      userId:      null,
      username:    null,
      displayName: null,
      setAuth: (token, userId, username, displayName) =>
        set({ token, userId, username, displayName }),
      clearAuth: () =>
        set({ token: null, userId: null, username: null, displayName: null }),

      // ── Canvas tool state (in-memory) ─────────────────
      activeTool: 'pen',
      color:      '#1a1a2e',
      lineWidth:  4,
      fillShape:  false,
      setTool:      (activeTool) => set({ activeTool }),
      setColor:     (color)      => set({ color }),
      setLineWidth: (lineWidth)  => set({ lineWidth }),
      setFillShape: (fillShape)  => set({ fillShape }),

      // ── Pending invites (in-memory) ───────────────────
      pendingInvites: [],
      addInvite:    (invite)    => set((s) => ({ pendingInvites: [...s.pendingInvites, invite] })),
      removeInvite: (sessionId) => set((s) => ({
        pendingInvites: s.pendingInvites.filter((i) => i.sessionId !== sessionId),
      })),
    }),
    {
      name: 'collabpaint-auth',
      partialize: (s) => ({
        token: s.token, userId: s.userId,
        username: s.username, displayName: s.displayName,
      }),
    }
  )
)
