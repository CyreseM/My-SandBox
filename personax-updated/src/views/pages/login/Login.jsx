import React, { useState } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

const Login = () => {
  const { t } = useTranslation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4 dark:bg-slate-950">
      <div className="w-full max-w-sm">
        {/* Logo */}
        <div className="mb-8 text-center">
          <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-600">
            <span className="text-xl font-bold text-white">PX</span>
          </div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">PersonaX Central</h1>
          <p className="mt-1 text-sm text-muted">{t('login', 'Sign in to your account')}</p>
        </div>

        <div className="card p-6">
          <div className="space-y-4">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
                Email
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@personax.io"
                className="input"
                autoComplete="email"
              />
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
                Password
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="input"
                autoComplete="current-password"
              />
            </div>
            <button className="btn-primary w-full justify-center py-2.5">
              {t('login', 'Sign In')}
            </button>
          </div>

          <p className="mt-5 text-center text-xs text-muted">
            {t('register', "Don't have an account?")}{' '}
            <Link to="/register" className="font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              {t('register', 'Register')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}

export default Login
