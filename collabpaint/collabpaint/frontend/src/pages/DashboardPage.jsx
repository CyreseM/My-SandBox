import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { createSessionApi, getSessionsApi, deleteSessionApi } from '../api/sessionsApi'
import { useAppStore } from '../store/useAppStore'
import s from './Dashboard.module.css'

export default function DashboardPage() {
  const [newName, setNewName] = useState('')
  const [creating, setCreating] = useState(false)
  const { displayName, username, clearAuth } = useAppStore((st) => ({ displayName: st.displayName, username: st.username, clearAuth: st.clearAuth }))
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data: sessions = [], isLoading } = useQuery({ queryKey: ['sessions'], queryFn: getSessionsApi })

  const createMut = useMutation({
    mutationFn: (name) => createSessionApi(name),
    onSuccess: (ses) => { qc.invalidateQueries({ queryKey: ['sessions'] }); navigate(`/canvas/${ses.id}`) },
    onError: () => toast.error('Could not create session.'),
  })
  const deleteMut = useMutation({
    mutationFn: deleteSessionApi,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sessions'] }); toast.success('Session deleted.') },
    onError: () => toast.error('Could not delete session.'),
  })

  const handleCreate = (e) => {
    e.preventDefault()
    createMut.mutate(newName.trim() || `Canvas ${new Date().toLocaleDateString()}`)
    setNewName(''); setCreating(false)
  }

  const fmt = (iso) => new Date(iso).toLocaleDateString('en-US', { month:'short', day:'numeric', year:'numeric' })

  return (
    <div className={s.page}>
      <header className={s.header}>
        <div className={s.headerInner}>
          <div className={s.logo}><span className={s.logoIcon}>🎨</span><span className={s.logoText}>CollabPaint</span></div>
          <div className={s.userArea}>
            <div className={s.avatar}>{displayName?.[0]?.toUpperCase() ?? 'U'}</div>
            <div className={s.userInfo}><span className={s.dn}>{displayName}</span><span className={s.un}>@{username}</span></div>
            <button className={s.logoutBtn} onClick={() => { clearAuth(); navigate('/login') }} title="Sign out">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            </button>
          </div>
        </div>
      </header>

      <main className={s.main}>
        <div className={s.topRow}>
          <div>
            <h1 className={s.title}>Your Canvases</h1>
            <p className={s.sub}>{sessions.length} session{sessions.length !== 1 ? 's' : ''}</p>
          </div>
          {!creating
            ? <button className={s.newBtn} onClick={() => setCreating(true)}>
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                New canvas
              </button>
            : <form className={s.createForm} onSubmit={handleCreate}>
                <input className={s.nameInput} placeholder="Canvas name…" value={newName} onChange={(e) => setNewName(e.target.value)} autoFocus />
                <button className={s.confirmBtn} type="submit" disabled={createMut.isPending}>{createMut.isPending ? '…' : 'Create'}</button>
                <button className={s.cancelBtn} type="button" onClick={() => setCreating(false)}>Cancel</button>
              </form>
          }
        </div>

        {isLoading ? (
          <div className={s.grid}>{[...Array(4)].map((_,i) => <div key={i} className={s.skeleton}/>)}</div>
        ) : sessions.length === 0 ? (
          <div className={s.empty}>
            <div className={s.emptyIcon}>🖼️</div>
            <h2 className={s.emptyTitle}>No canvases yet</h2>
            <p className={s.emptySub}>Create your first canvas to paint solo or invite friends.</p>
            <button className={s.newBtn} onClick={() => setCreating(true)}>Create first canvas</button>
          </div>
        ) : (
          <div className={s.grid}>
            {sessions.map((ses) => (
              <div key={ses.id} className={s.card}>
                <button className={s.preview} onClick={() => navigate(`/canvas/${ses.id}`)}>
                  {ses.canvasSnapshotBase64
                    ? <img src={ses.canvasSnapshotBase64} alt="preview" className={s.snap}/>
                    : <div className={s.noSnap}><span>✏️</span></div>
                  }
                  <div className={s.overlay}><span>Open →</span></div>
                </button>
                <div className={s.cardBody}>
                  <div className={s.cardTop}>
                    <h3 className={s.cardName}>{ses.name}</h3>
                    {ses.isCollaborative && <span className={s.badge}>Collab</span>}
                  </div>
                  <p className={s.cardMeta}>{fmt(ses.createdAt)} · {ses.participantCount} participant{ses.participantCount !== 1 ? 's' : ''}</p>
                  <div className={s.cardActions}>
                    <button className={s.openBtn} onClick={() => navigate(`/canvas/${ses.id}`)}>Open canvas</button>
                    <button className={s.delBtn} onClick={() => deleteMut.mutate(ses.id)} disabled={deleteMut.isPending} title="Delete">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
