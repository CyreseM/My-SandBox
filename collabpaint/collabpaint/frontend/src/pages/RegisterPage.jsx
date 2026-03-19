import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { registerApi } from '../api/authApi'
import { useAppStore } from '../store/useAppStore'
import s from './Auth.module.css'

export default function RegisterPage() {
  const [form, setForm] = useState({ displayName:'', username:'', email:'', password:'' })
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }))
  const setAuth = useAppStore((st) => st.setAuth)
  const navigate = useNavigate()

  const { mutate, isPending } = useMutation({
    mutationFn: () => registerApi(form.username, form.email, form.password, form.displayName),
    onSuccess: (d) => { setAuth(d.token, d.userId, d.username, d.displayName); toast.success(`Welcome, ${d.displayName}! Let's paint 🎨`); navigate('/dashboard') },
    onError:   ()  => toast.error('Registration failed. Username or email may be taken.'),
  })

  return (
    <div className={s.page}>
      <div className={s.bg}><div className={s.blob1}/><div className={s.blob2}/><div className={s.grid}/></div>
      <div className={s.card}>
        <div className={s.logo}><span className={s.logoIcon}>🎨</span><span className={s.logoText}>CollabPaint</span></div>
        <h1 className={s.title}>Create account</h1>
        <p className={s.sub}>Start painting with the world</p>
        <form className={s.form} onSubmit={(e)=>{ e.preventDefault(); mutate() }}>
          <div className={s.row}>
            <label className={s.label}>Display name<input className={s.input} placeholder="Alice" value={form.displayName} onChange={set('displayName')} required /></label>
            <label className={s.label}>Username<input className={s.input} placeholder="alice42" value={form.username} onChange={set('username')} required /></label>
          </div>
          <label className={s.label}>Email<input className={s.input} type="email" placeholder="alice@example.com" value={form.email} onChange={set('email')} required /></label>
          <label className={s.label}>Password<input className={s.input} type="password" placeholder="Min. 6 characters" value={form.password} onChange={set('password')} minLength={6} required /></label>
          <button className={s.btn} disabled={isPending}>{isPending ? <span className={s.spinner}/> : 'Create account →'}</button>
        </form>
        <p className={s.switch}>Have an account? <Link to="/login" className={s.link}>Sign in</Link></p>
      </div>
    </div>
  )
}
