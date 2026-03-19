import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { searchUsersApi } from '../../api/usersApi'
import { useAppStore } from '../../store/useAppStore'
import s from './UserSearchPanel.module.css'

export default function UserSearchPanel({ onInvite, inviting }) {
  const [query, setQuery] = useState('')
  const myId = useAppStore((st) => st.userId)

  const { data: users = [], isFetching } = useQuery({
    queryKey: ['userSearch', query],
    queryFn:  () => searchUsersApi(query),
    enabled:  query.trim().length >= 2,
    staleTime: 30_000,
  })

  const filtered = users.filter((u) => u.id !== myId)

  return (
    <div className={s.panel}>
      <div className={s.searchBar}>
        <svg className={s.searchIcon} width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input
          className={s.input}
          placeholder="Search users…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
      </div>

      {query.length > 0 && query.length < 2 && <p className={s.hint}>Type at least 2 characters</p>}
      {isFetching && <div className={s.loadRow}><span className={s.spinner}/></div>}
      {!isFetching && filtered.length === 0 && query.length >= 2 && <p className={s.hint}>No users found for "{query}"</p>}

      <ul className={s.list}>
        {filtered.map((u) => (
          <li key={u.id} className={s.row}>
            <div className={s.avatar}>{u.displayName?.[0]?.toUpperCase() ?? 'U'}</div>
            <div className={s.info}>
              <span className={s.dn}>{u.displayName}</span>
              <span className={s.un}>@{u.username}</span>
            </div>
            <button
              className={s.inviteBtn}
              onClick={() => onInvite(u.id, u.username)}
              disabled={inviting === u.id}
            >
              {inviting === u.id ? <span className={s.spinner}/> : <>
                <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Invite
              </>}
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}
