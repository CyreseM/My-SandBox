import React, { useEffect } from 'react'

const CountryForm = ({ visible, setVisible }) => {
  // Close on Escape key
  useEffect(() => {
    const handler = (e) => { if (e.key === 'Escape') setVisible(false) }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [setVisible])

  return (
    <>
      {/* Backdrop */}
      {visible && (
        <div
          className="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm"
          onClick={() => setVisible(false)}
        />
      )}

      {/* Drawer */}
      <div
        className={`fixed inset-y-0 right-0 z-50 flex w-full max-w-sm flex-col bg-white shadow-2xl
                    transition-transform duration-300 dark:bg-slate-900
                    ${visible ? 'translate-x-0' : 'translate-x-full'}`}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b px-4 py-3 shadow-sm dark:border-slate-700">
          <h2 className="text-sm font-semibold text-slate-900 dark:text-white">Activate Country</h2>
          <button
            onClick={() => setVisible(false)}
            className="flex h-7 w-7 items-center justify-center rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-700 dark:hover:bg-slate-800"
            aria-label="Close"
          >
            <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto px-4 py-4 space-y-3">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300">
              Country Name
            </label>
            <select className="input text-sm h-8">
              <option value="">Select Country</option>
              <option value="ghana">Ghana</option>
              <option value="nigeria">Nigeria</option>
              <option value="dr">Dominican Republic</option>
              <option value="drc">Democratic Republic of Congo</option>
            </select>
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300">
              Currency Name
            </label>
            <input type="text" disabled placeholder="Auto-filled" className="input text-sm h-8 opacity-60 cursor-not-allowed" />
          </div>

          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300">Symbol</label>
              <input type="text" disabled placeholder="$" className="input text-sm h-8 opacity-60 cursor-not-allowed" />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300">Code</label>
              <input type="text" disabled placeholder="GHS" className="input text-sm h-8 opacity-60 cursor-not-allowed" />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300">Status</label>
              <label className="mt-2 flex cursor-pointer items-center gap-2">
                <div className="relative">
                  <input type="checkbox" className="sr-only peer" defaultChecked />
                  <div className="h-5 w-9 rounded-full bg-slate-200 peer-checked:bg-brand-600 transition-colors" />
                  <div className="absolute left-0.5 top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform peer-checked:translate-x-4" />
                </div>
                <span className="text-xs text-slate-600 dark:text-slate-400">Active</span>
              </label>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 border-t px-4 py-3 dark:border-slate-700">
          <button
            onClick={() => setVisible(false)}
            className="btn-secondary text-sm h-8 px-4"
          >
            Cancel
          </button>
          <button className="btn-primary text-sm h-8 px-4">
            Save
          </button>
        </div>
      </div>
    </>
  )
}

export default CountryForm
