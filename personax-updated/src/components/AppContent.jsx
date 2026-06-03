import React, { Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useSelector } from 'react-redux'
import AppBreadcrumb from './AppBreadcrumb'
import routes from '../routes'

const LoadingSpinner = () => (
  <div className="flex items-center justify-center py-16">
    <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-200 border-t-brand-600" />
  </div>
)

const AppContent = () => {
  const sidebarCollapsed = useSelector((s) => s.sidebarCollapsed)
  const sidebarWidth = sidebarCollapsed ? '64px' : '260px'

  return (
    <main
      className="flex-1 overflow-auto"
      style={{ paddingLeft: sidebarWidth }}
    >
      <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <AppBreadcrumb />
        <Suspense fallback={<LoadingSpinner />}>
          <Routes>
            {routes.map((route, idx) =>
              route.element ? (
                <Route
                  key={idx}
                  path={route.path}
                  exact={route.exact}
                  name={route.name}
                  element={<route.element />}
                />
              ) : null
            )}
            <Route path="/" element={<Navigate to="dashboard" replace />} />
          </Routes>
        </Suspense>
      </div>
    </main>
  )
}

export default React.memo(AppContent)
