import React from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

const Register = () => {
  const { t } = useTranslation()
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4 dark:bg-slate-950">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-600">
            <span className="text-xl font-bold text-white">PX</span>
          </div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Create Account</h1>
        </div>
        <div className="card p-6 space-y-4">
          {['Full Name', 'Email', 'Password'].map((field) => (
            <div key={field}>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{field}</label>
              <input type={field === 'Password' ? 'password' : field === 'Email' ? 'email' : 'text'} className="input" />
            </div>
          ))}
          <button className="btn-primary w-full justify-center py-2.5">{t('register', 'Register')}</button>
          <p className="text-center text-xs text-muted">
            Already have an account?{' '}
            <Link to="/login" className="font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              {t('login', 'Sign In')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}

export default Register
