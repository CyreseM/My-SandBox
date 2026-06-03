import React, { Suspense, useEffect } from 'react'
import { HashRouter, Route, Routes } from 'react-router-dom'
import { useSelector } from 'react-redux'
import { useTheme } from './hooks/useTheme'

// Import i18n to initialise it
import './i18n'

const DefaultLayout = React.lazy(() => import('./layout/DefaultLayout'))
const Login         = React.lazy(() => import('./views/pages/login/Login'))
const Register      = React.lazy(() => import('./views/pages/register/Register'))
const Page404       = React.lazy(() => import('./views/pages/page404/Page404'))
const Page500       = React.lazy(() => import('./views/pages/page500/Page500'))

const LoadingScreen = () => (
  <div className="flex min-h-screen items-center justify-center bg-slate-50 dark:bg-slate-950">
    <div className="h-9 w-9 animate-spin rounded-full border-4 border-brand-200 border-t-brand-600" />
  </div>
)

const App = () => {
  const storedTheme = useSelector((s) => s.theme)
  const { setMode }  = useTheme()

  // Initialise theme from Redux state on first load
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.href.split('?')[1])
    const queryTheme = urlParams.get('theme')
    if (queryTheme && ['light', 'dark', 'auto'].includes(queryTheme)) {
      setMode(queryTheme)
    } else if (storedTheme) {
      setMode(storedTheme)
    }
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <HashRouter>
      <Suspense fallback={<LoadingScreen />}>
        <Routes>
          <Route path="/login"    element={<Login />}    />
          <Route path="/register" element={<Register />} />
          <Route path="/404"      element={<Page404 />}  />
          <Route path="/500"      element={<Page500 />}  />
          <Route path="*"         element={<DefaultLayout />} />
        </Routes>
      </Suspense>
    </HashRouter>
  )
}

export default App
