import React from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import routes from '../routes'

const AppBreadcrumb = () => {
  const { t } = useTranslation()
  const location = useLocation()

  const getCrumbs = () => {
    const crumbs = [{ name: t('home', 'Home'), path: '/' }]
    const parts = location.pathname.split('/').filter(Boolean)
    let cumPath = ''
    parts.forEach((part) => {
      cumPath += '/' + part
      const route = routes.find((r) => r.path === cumPath)
      if (route) crumbs.push({ name: route.name || part, path: cumPath })
    })
    return crumbs
  }

  const crumbs = getCrumbs()
  if (crumbs.length <= 1) return null

  return (
    <nav className="mb-4 flex" aria-label="Breadcrumb">
      <ol className="flex items-center gap-1 text-sm">
        {crumbs.map((crumb, idx) => (
          <li key={crumb.path} className="flex items-center gap-1">
            {idx > 0 && (
              <svg className="h-3.5 w-3.5 flex-shrink-0 text-slate-400" viewBox="0 0 24 24" fill="currentColor">
                <path d="M8.59 16.59 13.17 12 8.59 7.41 10 6l6 6-6 6z" />
              </svg>
            )}
            {idx === crumbs.length - 1 ? (
              <span className="font-medium text-slate-900 dark:text-white">
                {typeof crumb.name === 'string' ? crumb.name : crumb.name}
              </span>
            ) : (
              <Link
                to={crumb.path}
                className="text-slate-500 hover:text-slate-900 dark:hover:text-white transition-colors"
              >
                {crumb.name}
              </Link>
            )}
          </li>
        ))}
      </ol>
    </nav>
  )
}

export default AppBreadcrumb
