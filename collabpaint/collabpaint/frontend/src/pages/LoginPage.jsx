import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { loginApi } from '../api/authApi'
import { useAppStore } from '../store/useAppStore'
import s from './Auth.module.css'

export default function LoginPage() {
  const [email, setEmail]       = useState('')
  const [password, setPassword] = useState('')
  const setAuth = useAppStore((st) => st.setAuth)
  const navigate = useNavigate()

  const { mutate, isPending } = useMutation({
    mutationFn: () => loginApi(email, password),
    onSuccess: (d) => { setAuth(d.token, d.userId, d.username, d.displayName); toast.success(`Welcome back, ${d.displayName}!`); navigate('/dashboard') },
    onError:   ()  => toast.error('Invalid email or password.'),
  })

  return (
    <div className={s.page}>
      <div className={s.bg}><div className={s.blob1}/><div className={s.blob2}/><div className={s.grid}/></div>
      <div className={s.card}>
        <div className={s.logo}><span className={s.logoIcon}>🎨</span><span className={s.logoText}>CollabPaint</span></div>
        <h1 className={s.title}>Sign in</h1>
        <p className={s.sub}>Jump back into your canvas</p>
        <form className={s.form} onSubmit={(e) => { e.preventDefault(); mutate() }}>
          <label className={s.label}>Email
            <input className={s.input} type="email" placeholder="you@example.com" value={email} onChange={(e)=>setEmail(e.target.value)} required autoComplete="email" />
          </label>
          <label className={s.label}>Password
            <input className={s.input} type="password" placeholder="••••••••" value={password} onChange={(e)=>setPassword(e.target.value)} required autoComplete="current-password" />
          </label>
          <button className={s.btn} disabled={isPending}>{isPending ? <span className={s.spinner}/> : 'Sign in →'}</button>
        </form>
        <p className={s.switch}>No account? <Link to="/register" className={s.link}>Create one</Link></p>
      </div>
    </div>
  )
}
