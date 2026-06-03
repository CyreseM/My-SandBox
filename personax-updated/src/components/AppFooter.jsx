import React from 'react'
import { useSelector } from 'react-redux'

const AppFooter = () => {
  const sidebarCollapsed = useSelector((s) => s.sidebarCollapsed)
  const sidebarWidth = sidebarCollapsed ? '64px' : '260px'

  return (
    <footer
      className="border-t bg-white py-3 text-xs text-slate-500
                 dark:bg-slate-900 dark:border-slate-800 dark:text-slate-500"
      style={{ paddingLeft: sidebarWidth }}
    >
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <span>
          &copy; {new Date().getFullYear()}{' '}
          <a href="#" className="hover:text-slate-900 dark:hover:text-white transition-colors">
            PersonaX Central
          </a>
        </span>
        <span>Powered by Tailwind CSS</span>
      </div>
    </footer>
  )
}

export default React.memo(AppFooter)
