import { useState, useCallback, useRef, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { getSessionApi, getStrokesApi, saveSnapshotApi } from '../api/sessionsApi'
import { useAppStore } from '../store/useAppStore'
import { useSignalR } from '../hooks/useSignalR'
import { PARTICIPANT_COLORS } from '../hooks/useCanvas'
import DrawingCanvas from '../components/Canvas/DrawingCanvas'
import Toolbar       from '../components/Canvas/Toolbar'
import UserSearchPanel from '../components/Invite/UserSearchPanel'
import s from './Canvas.module.css'

export default function CanvasPage() {
  const { sessionId } = useParams()
  const navigate = useNavigate()
  const { userId, username, displayName } = useAppStore()

  const [participants, setParticipants] = useState([])
  const [remoteStroke, setRemoteStroke]  = useState(null)
  const [remoteCursor, setRemoteCursor]  = useState(null)
  const [clearSignal, setClearSignal]    = useState(0)
  const [inviting, setInviting]          = useState(null)
  const [sideTab, setSideTab]            = useState('participants')
  const [sideOpen, setSideOpen]          = useState(true)
  const snapshotFnRef = useRef(null)

  const { data: session }                          = useQuery({ queryKey:['session', sessionId],   queryFn: () => getSessionApi(sessionId),  enabled: !!sessionId })
  const { data: initialStrokes = [] }              = useQuery({ queryKey:['strokes', sessionId],    queryFn: () => getStrokesApi(sessionId),   enabled: !!sessionId })

  const { status, sendStroke, sendClear, sendCursorMove, inviteUser } = useSignalR({
    sessionId,
    onStrokeReceived: useCallback((st) => setRemoteStroke({ ...st, _t: Date.now() }), []),
    onClearReceived:  useCallback(() => setClearSignal((n) => n + 1), []),
    onCursorMove:     useCallback((c)  => setRemoteCursor(c), []),
    onUserJoined: useCallback((p) => {
      setParticipants((prev) => {
        if (prev.find((x) => x.userId === p.userId)) return prev
        return [...prev, { ...p, color: PARTICIPANT_COLORS[prev.length % PARTICIPANT_COLORS.length] }]
      })
      toast(`${p.displayName || p.username} joined 🎨`)
    }, []),
    onUserLeft: useCallback((uid) => {
      setParticipants((prev) => {
        const who = prev.find((x) => x.userId === uid)
        if (who) toast(`${who.displayName || who.username} left`)
        return prev.filter((x) => x.userId !== uid)
      })
    }, []),
    onInviteAccepted: useCallback((_uid, uname) => toast.success(`${uname} accepted your invite!`), []),
    onInviteDeclined: useCallback(() => toast('Invite was declined', { icon:'👋' }), []),
  })

  /* add self to participants list */
  useEffect(() => {
    if (userId && username) {
      setParticipants([{ userId, username, displayName: displayName || username, joinedAt: new Date().toISOString(), color: PARTICIPANT_COLORS[0] }])
    }
  }, [userId, username, displayName])

  /* auto-save snapshot every 60 s */
  useEffect(() => {
    if (!sessionId) return
    const id = setInterval(() => {
      const snap = snapshotFnRef.current?.()
      if (snap) saveSnapshotApi(sessionId, snap).catch(() => {})
    }, 60_000)
    return () => clearInterval(id)
  }, [sessionId])

  const handleStrokeComplete  = useCallback((st) => sendStroke(st), [sendStroke])
  const handleStrokeProgress  = useCallback((st) => sendStroke(st), [sendStroke])
  const handleCursorMove      = useCallback((x, y) => sendCursorMove(x, y), [sendCursorMove])
  const handleClear           = useCallback(() => { setClearSignal((n) => n + 1); sendClear() }, [sendClear])
  const handleInvite          = useCallback((uid, uname) => {
    setInviting(uid); inviteUser(uid)
    toast.success(`Invite sent to @${uname}!`)
    setTimeout(() => setInviting(null), 2000)
  }, [inviteUser])

  const statusColor = { connected:'#2a9d8f', reconnecting:'#e9a14a', disconnected:'#e63946', connecting:'#e9a14a' }[status] || '#888'

  return (
    <div className={s.page}>
      {/* Top bar */}
      <header className={s.topbar}>
        <button className={s.backBtn} onClick={() => navigate('/dashboard')} title="Back">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="15 18 9 12 15 6"/></svg>
        </button>
        <div className={s.sessionInfo}>
          <h1 className={s.sessionName}>{session?.name ?? '…'}</h1>
          {session?.isCollaborative && <span className={s.collabBadge}>Collaborative</span>}
        </div>
        <div className={s.topRight}>
          <span className={s.statusDot} style={{ background: statusColor }} title={status}/>
          <span className={s.statusLabel}>{status}</span>
          <div className={s.avatarStack}>
            {participants.slice(0, 5).map((p) => (
              <div key={p.userId} className={s.pAvatar} style={{ background: p.color }} title={p.displayName || p.username}>
                {(p.displayName || p.username)[0]?.toUpperCase()}
              </div>
            ))}
            {participants.length > 5 && <div className={s.pAvatar} style={{ background:'#888' }}>+{participants.length - 5}</div>}
          </div>
          <button className={s.sideToggle} onClick={() => setSideOpen((v) => !v)} title="Toggle sidebar">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="15" y1="3" x2="15" y2="21"/></svg>
          </button>
        </div>
      </header>

      <div className={s.workspace}>
        <Toolbar onClear={handleClear}/>

        <DrawingCanvas
          onStrokeComplete={handleStrokeComplete}
          onStrokeInProgress={handleStrokeProgress}
          onCursorMove={handleCursorMove}
          remoteStroke={remoteStroke}
          remoteCursor={remoteCursor}
          clearSignal={clearSignal}
          replayStrokes={initialStrokes}
          onSnapshotReady={(fn) => { snapshotFnRef.current = fn }}
        />

        {sideOpen && (
          <aside className={s.sidebar}>
            <div className={s.tabs}>
              <button className={`${s.tab} ${sideTab==='participants' ? s.activeTab:''}`} onClick={() => setSideTab('participants')}>
                Participants <span className={s.badge}>{participants.length}</span>
              </button>
              <button className={`${s.tab} ${sideTab==='invite' ? s.activeTab:''}`} onClick={() => setSideTab('invite')}>
                Invite
              </button>
            </div>

            <div className={s.sideContent}>
              {sideTab === 'participants' && (
                <ul className={s.pList}>
                  {participants.map((p) => (
                    <li key={p.userId} className={s.pRow}>
                      <div className={s.pAvatarBig} style={{ background: p.color }}>{(p.displayName || p.username)[0]?.toUpperCase()}</div>
                      <div className={s.pInfo}>
                        <span className={s.pName}>{p.displayName || p.username}{p.userId === userId && <span className={s.you}> (you)</span>}</span>
                        <span className={s.pUn}>@{p.username}</span>
                      </div>
                      <div className={s.pDot} style={{ background: p.color }}/>
                    </li>
                  ))}
                </ul>
              )}
              {sideTab === 'invite' && (
                <div className={s.inviteWrap}>
                  <p className={s.inviteHint}>Search and invite users to paint with you in real time.</p>
                  <UserSearchPanel onInvite={handleInvite} inviting={inviting}/>
                </div>
              )}
            </div>
          </aside>
        )}
      </div>
    </div>
  )
}
