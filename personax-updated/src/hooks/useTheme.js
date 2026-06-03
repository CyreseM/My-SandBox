/**
 * useTheme – manages light / dark / auto colour mode.
 *
 * Persists the chosen mode in localStorage under the key
 * "personax-theme" so it survives page reloads.
 *
 * "auto" follows the OS preference via prefers-color-scheme.
 */
import { useCallback, useEffect, useState } from 'react'

const STORAGE_KEY = 'personax-theme'

function resolveEffectiveTheme(mode) {
  if (mode === 'auto') {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  }
  return mode
}

export function useTheme() {
  const [mode, setModeState] = useState(() => localStorage.getItem(STORAGE_KEY) || 'light')

  // Apply / remove the "dark" class on <html>
  useEffect(() => {
    const effective = resolveEffectiveTheme(mode)
    document.documentElement.classList.toggle('dark', effective === 'dark')
  }, [mode])

  // Keep in sync with OS preference when mode === 'auto'
  useEffect(() => {
    if (mode !== 'auto') return
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = () => {
      document.documentElement.classList.toggle('dark', mq.matches)
    }
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [mode])

  const setMode = useCallback((newMode) => {
    localStorage.setItem(STORAGE_KEY, newMode)
    setModeState(newMode)
  }, [])

  return { mode, setMode, isDark: resolveEffectiveTheme(mode) === 'dark' }
}
