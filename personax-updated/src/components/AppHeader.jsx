import React, { useRef, useEffect, useState } from 'react'
import { useSelector, useDispatch } from 'react-redux'
import { useTranslation } from 'react-i18next'
import { useTheme } from '../hooks/useTheme'
import NavIcon from './NavIcon'

/* ── Generic dropdown ─────────────────────────────────────────────────── */
const Dropdown = ({ trigger, children }) => {
  const [open, setOpen] = useState(false)
  const ref = useRef()

  useEffect(() => {
    const handler = (e) => { if (ref.current && !ref.current.contains(e.target)) setOpen(false) }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex h-9 w-9 items-center justify-center rounded-lg text-slate-500 transition hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-white"
      >
        {trigger}
      </button>
      {open && (
        <div className="absolute right-0 z-50 mt-1 min-w-[160px] rounded-xl border bg-white py-1 shadow-lg dark:border-slate-700 dark:bg-slate-900">
          {children(() => setOpen(false))}
        </div>
      )}
    </div>
  )
}

/* ── Dropdown item ────────────────────────────────────────────────────── */
const DropItem = ({ active, onClick, children }) => (
  <button
    onClick={onClick}
    className={`flex w-full items-center gap-2 px-4 py-2 text-sm transition
      ${active
        ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/30 dark:text-brand-400'
        : 'text-slate-700 hover:bg-slate-50 dark:text-slate-300 dark:hover:bg-slate-800'
      }`}
  >
    {children}
  </button>
)

/* ── Flag emojis for language selector ───────────────────────────────── */
const flags = { en: '🇬🇧', es: '🇪🇸', pl: '🇵🇱' }
const langLabels = { en: 'English', es: 'Español', pl: 'Polski' }

/* ── AppHeader ────────────────────────────────────────────────────────── */
const AppHeader = () => {
  const dispatch = useDispatch()
  const sidebarShow = useSelector((s) => s.sidebarShow)
  const sidebarCollapsed = useSelector((s) => s.sidebarCollapsed)
  const { t, i18n } = useTranslation()
  const { mode, setMode } = useTheme()
  const headerRef = useRef()

  // Sticky shadow on scroll
  useEffect(() => {
    const handler = () => {
      if (headerRef.current) {
        headerRef.current.classList.toggle('shadow-sm', document.documentElement.scrollTop > 0)
      }
    }
    document.addEventListener('scroll', handler, { passive: true })
    return () => document.removeEventListener('scroll', handler)
  }, [])

  const sidebarWidth = sidebarCollapsed ? '64px' : '260px'

  const themeIcon = {
    light: <NavIcon name="sun" className="h-5 w-5" />,
    dark:  <NavIcon name="moon" className="h-5 w-5" />,
    auto:  <NavIcon name="contrast" className="h-5 w-5" />,
  }

  return (
    <header
      ref={headerRef}
      className="sticky top-0 z-20 flex h-14 items-center border-b bg-white/80 backdrop-blur-md
                 transition-shadow dark:bg-slate-900/80 dark:border-slate-800"
      style={{ paddingLeft: `calc(${sidebarWidth} + 1rem)` }}
    >
      {/* Mobile menu toggle */}
      <button
        className="mr-3 flex h-9 w-9 items-center justify-center rounded-lg text-slate-500
                   transition hover:bg-slate-100 hover:text-slate-900
                   dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-white lg:hidden"
        onClick={() => dispatch({ type: 'set', sidebarShow: !sidebarShow })}
        aria-label="Toggle sidebar"
      >
        <NavIcon name="menu" className="h-5 w-5" />
      </button>

      {/* Search */}
      <div className="relative hidden flex-1 max-w-sm sm:flex">
        <NavIcon
          name="search"
          className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
        />
        <input
          type="search"
          placeholder={t('search', 'Search...')}
          className="input pl-9 h-9 text-sm"
          aria-label="Search"
        />
      </div>

      {/* Right actions */}
      <div className="ml-auto flex items-center gap-1 pr-4">

        {/* Notification bell */}
        <button className="flex h-9 w-9 items-center justify-center rounded-lg text-slate-500
                           transition hover:bg-slate-100 hover:text-slate-900 relative
                           dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-white">
          <NavIcon name="bell" className="h-5 w-5" />
          <span className="absolute top-1.5 right-1.5 h-2 w-2 rounded-full bg-red-500 ring-2 ring-white dark:ring-slate-900" />
        </button>

        {/* Divider */}
        <div className="mx-1 h-6 w-px bg-slate-200 dark:bg-slate-700" />

        {/* Language switcher */}
        <Dropdown trigger={<NavIcon name="language" className="h-5 w-5" />}>
          {(close) => ['en', 'es', 'pl'].map((lng) => (
            <DropItem
              key={lng}
              active={i18n.language === lng}
              onClick={() => { i18n.changeLanguage(lng); close() }}
            >
              <span className="text-base">{flags[lng]}</span>
              <span>{langLabels[lng]}</span>
            </DropItem>
          ))}
        </Dropdown>

        {/* Theme switcher */}
        <Dropdown trigger={themeIcon[mode]}>
          {(close) => [
            { value: 'light', icon: 'sun',      label: t('light', 'Light') },
            { value: 'dark',  icon: 'moon',     label: t('dark', 'Dark') },
            { value: 'auto',  icon: 'contrast', label: 'Auto' },
          ].map(({ value, icon, label }) => (
            <DropItem
              key={value}
              active={mode === value}
              onClick={() => { setMode(value); close() }}
            >
              <NavIcon name={icon} className="h-4 w-4" />
              <span>{label}</span>
            </DropItem>
          ))}
        </Dropdown>

        {/* Divider */}
        <div className="mx-1 h-6 w-px bg-slate-200 dark:bg-slate-700" />

        {/* User avatar dropdown */}
        <Dropdown
          trigger={
            <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-white text-xs font-bold">
              JD
            </div>
          }
        >
          {(close) => (
            <>
              <div className="px-4 py-2 border-b dark:border-slate-700">
                <p className="text-sm font-semibold text-slate-900 dark:text-white">John Doe</p>
                <p className="text-xs text-slate-500">john@personax.io</p>
              </div>
              {[
                { icon: 'profile',  label: t('profile', 'Profile') },
                { icon: 'settings', label: t('settings', 'Settings') },
              ].map(({ icon, label }) => (
                <DropItem key={label} active={false} onClick={close}>
                  <NavIcon name={icon} className="h-4 w-4" />
                  <span>{label}</span>
                </DropItem>
              ))}
              <div className="my-1 h-px bg-slate-100 dark:bg-slate-700" />
              <DropItem active={false} onClick={close}>
                <NavIcon name="logout" className="h-4 w-4 text-red-500" />
                <span className="text-red-600 dark:text-red-400">{t('logout', 'Logout')}</span>
              </DropItem>
            </>
          )}
        </Dropdown>
      </div>
    </header>
  )
}

export default AppHeader
