import React from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { useSelector, useDispatch } from 'react-redux'
import { useTranslation } from 'react-i18next'
import NavIcon from './NavIcon'
import nav from '../_nav'

/* ── Logo SVG (inline, no external dep) ──────────────────────────────── */
const Logo = ({ collapsed }) => (
  <div className="flex items-center gap-3 overflow-hidden">
    <div className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-lg bg-brand-600">
      <span className="text-sm font-bold text-white">PX</span>
    </div>
    {!collapsed && (
      <span className="truncate text-base font-bold tracking-tight text-white">
        PersonaX
        <span className="text-brand-400"> Central</span>
      </span>
    )}
  </div>
)

/* ── Badge ────────────────────────────────────────────────────────────── */
const BadgePill = ({ color, text }) => {
  const colours = {
    info:   'bg-sky-500/20 text-sky-300',
    danger: 'bg-red-500/20 text-red-300',
  }
  return (
    <span className={`ml-auto rounded-full px-2 py-0.5 text-[10px] font-bold uppercase ${colours[color] ?? colours.info}`}>
      {text}
    </span>
  )
}

/* ── Sidebar nav item ─────────────────────────────────────────────────── */
const SidebarItem = ({ item, collapsed }) => {
  const { t } = useTranslation()
  const location = useLocation()
  const isActive = location.pathname === item.to || location.pathname.startsWith(item.to + '/')

  return (
    <NavLink
      to={item.to}
      title={collapsed ? t(item.label, item.label) : undefined}
      className={`nav-item ${isActive ? 'active' : ''}`}
    >
      <NavIcon name={item.icon} />
      {!collapsed && (
        <>
          <span className="truncate">{t(item.label, item.label)}</span>
          {item.badge && <BadgePill {...item.badge} />}
        </>
      )}
    </NavLink>
  )
}

/* ── Section title ────────────────────────────────────────────────────── */
const SidebarTitle = ({ label, collapsed }) => {
  if (collapsed) return <div className="my-1 h-px mx-2 bg-white/10" />
  return (
    <p className="mx-5 mt-4 mb-1 text-[10px] font-semibold uppercase tracking-widest text-slate-500">
      {label}
    </p>
  )
}

/* ── Main sidebar ─────────────────────────────────────────────────────── */
const AppSidebar = () => {
  const dispatch = useDispatch()
  const sidebarShow = useSelector((s) => s.sidebarShow)
  const sidebarCollapsed = useSelector((s) => s.sidebarCollapsed)

  return (
    <>
      {/* Mobile overlay */}
      {sidebarShow && (
        <div
          className="fixed inset-0 z-20 bg-black/50 lg:hidden"
          onClick={() => dispatch({ type: 'set', sidebarShow: false })}
        />
      )}

      <aside
        className={`
          sidebar fixed inset-y-0 left-0 z-30 flex flex-col
          transition-all duration-200
          ${sidebarShow ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
          ${sidebarCollapsed ? 'collapsed' : ''}
        `}
      >
        {/* Brand */}
        <div className="flex h-14 flex-shrink-0 items-center justify-between border-b border-white/10 px-4">
          <Logo collapsed={sidebarCollapsed} />
          {!sidebarCollapsed && (
            <button
              onClick={() => dispatch({ type: 'set', sidebarCollapsed: true })}
              className="rounded-md p-1 text-slate-400 hover:text-white lg:flex hidden"
              aria-label="Collapse sidebar"
            >
              <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
                <path d="M15.41 7.41 14 6l-6 6 6 6 1.41-1.41L10.83 12z" />
              </svg>
            </button>
          )}
          {sidebarCollapsed && (
            <button
              onClick={() => dispatch({ type: 'set', sidebarCollapsed: false })}
              className="hidden rounded-md p-1 text-slate-400 hover:text-white lg:flex"
              aria-label="Expand sidebar"
            >
              <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
                <path d="M10 6 8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z" />
              </svg>
            </button>
          )}
        </div>

        {/* Nav */}
        <nav className="flex-1 overflow-y-auto py-2 scrollbar-hidden">
          {nav.map((item, idx) =>
            item.type === 'title' ? (
              <SidebarTitle key={idx} label={item.label} collapsed={sidebarCollapsed} />
            ) : (
              <SidebarItem key={item.key} item={item} collapsed={sidebarCollapsed} />
            )
          )}
        </nav>

        {/* Footer – version */}
        {!sidebarCollapsed && (
          <div className="flex-shrink-0 border-t border-white/10 px-4 py-3 text-[11px] text-slate-500">
            v1.0.0 · PersonaX Central
          </div>
        )}
      </aside>
    </>
  )
}

export default React.memo(AppSidebar)
