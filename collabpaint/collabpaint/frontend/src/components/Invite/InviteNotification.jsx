import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { useSignalR } from '../../hooks/useSignalR'
import { useAppStore } from '../../store/useAppStore'

export default function InviteNotification() {
  const navigate = useNavigate()
  const { addInvite, removeInvite } = useAppStore()

  const { acceptInvite, declineInvite } = useSignalR({
    onInviteReceived: (invite) => {
      addInvite(invite)

      toast(
        (t) => (
          <div style={{ display:'flex', flexDirection:'column', gap:10, minWidth:240 }}>
            <div style={{ lineHeight:1.4 }}>
              <strong style={{ fontWeight:700 }}>{invite.fromUsername}</strong>
              {' invited you to '}
              <em style={{ opacity:.8 }}>{invite.sessionName}</em>
            </div>
            <div style={{ display:'flex', gap:8 }}>
              <button
                onClick={() => {
                  acceptInvite(invite.sessionId)
                  removeInvite(invite.sessionId)
                  navigate(`/canvas/${invite.sessionId}`)
                  toast.dismiss(t.id)
                }}
                style={{ flex:1, background:'#5b5bd6', color:'#fff', border:'none', borderRadius:6, padding:'7px 0', fontFamily:'inherit', fontWeight:600, cursor:'pointer', fontSize:13 }}
              >Accept</button>
              <button
                onClick={() => {
                  declineInvite(invite.sessionId)
                  removeInvite(invite.sessionId)
                  toast.dismiss(t.id)
                }}
                style={{ flex:1, background:'rgba(255,255,255,.1)', color:'rgba(255,255,255,.7)', border:'1px solid rgba(255,255,255,.15)', borderRadius:6, padding:'7px 0', fontFamily:'inherit', fontWeight:600, cursor:'pointer', fontSize:13 }}
              >Decline</button>
            </div>
          </div>
        ),
        {
          id: `invite-${invite.sessionId}`,
          duration: 30_000,
          style: { background:'#1a1a2e', color:'#fff', fontFamily:"'DM Sans',sans-serif", borderRadius:12, padding:'14px 16px', maxWidth:320 },
        }
      )
    },
  })

  return null
}
