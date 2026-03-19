import { useEffect, useRef, useState, useCallback } from 'react'
import * as signalR from '@microsoft/signalr'
import { useAppStore } from '../store/useAppStore'

const BASE = import.meta.env.VITE_API_BASE_URL || ''

export function useSignalR({
  sessionId,
  onStrokeReceived,
  onClearReceived,
  onCursorMove,
  onUserJoined,
  onUserLeft,
  onInviteReceived,
  onInviteAccepted,
  onInviteDeclined,
}) {
  const token = useAppStore((s) => s.token)
  const connRef = useRef(null)
  const [status, setStatus] = useState('disconnected')

  useEffect(() => {
    if (!token) return

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE}/hubs/paint?access_token=${token}`)
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    conn.onreconnecting(() => setStatus('reconnecting'))
    conn.onreconnected(() => setStatus('connected'))
    conn.onclose(() => setStatus('disconnected'))

    conn.on('ReceiveStroke',     (s)        => onStrokeReceived?.(s))
    conn.on('ReceiveClear',      ()         => onClearReceived?.())
    conn.on('ReceiveCursorMove', (c)        => onCursorMove?.(c))
    conn.on('UserJoined',        (p)        => onUserJoined?.(p))
    conn.on('UserLeft',          (uid)      => onUserLeft?.(uid))
    conn.on('InviteReceived',    (inv)      => onInviteReceived?.(inv))
    conn.on('InviteAccepted',    (uid, un)  => onInviteAccepted?.(uid, un))
    conn.on('InviteDeclined',    (uid)      => onInviteDeclined?.(uid))

    setStatus('connecting')
    conn.start()
      .then(() => {
        setStatus('connected')
        connRef.current = conn
        if (sessionId) conn.invoke('JoinSession', sessionId).catch(console.error)
      })
      .catch((e) => { console.error('SignalR error:', e); setStatus('disconnected') })

    return () => {
      if (sessionId) conn.invoke('LeaveSession', sessionId).catch(() => {})
      conn.stop()
      connRef.current = null
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, sessionId])

  const sendStroke     = useCallback((s)    => connRef.current?.invoke('SendStroke', sessionId, s).catch(console.error),     [sessionId])
  const sendClear      = useCallback(()     => connRef.current?.invoke('SendClear', sessionId).catch(console.error),          [sessionId])
  const sendCursorMove = useCallback((x, y) => connRef.current?.invoke('SendCursorMove', sessionId, x, y).catch(console.error), [sessionId])
  const inviteUser     = useCallback((uid)  => connRef.current?.invoke('InviteUser', uid, sessionId).catch(console.error),    [sessionId])
  const acceptInvite   = useCallback((sid)  => connRef.current?.invoke('AcceptInvite', sid).catch(console.error),             [])
  const declineInvite  = useCallback((sid)  => connRef.current?.invoke('DeclineInvite', sid).catch(console.error),            [])

  return { status, sendStroke, sendClear, sendCursorMove, inviteUser, acceptInvite, declineInvite }
}
