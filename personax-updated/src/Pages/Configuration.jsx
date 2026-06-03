import React, { useState } from 'react'

const allSegments = [
  { id: 'departments',    label: 'Departments',    active: true  },
  { id: 'divisions',      label: 'Divisions',      active: true  },
  { id: 'employeetypes',  label: 'Employee Types', active: false },
  { id: 'location',       label: 'Location',       active: false },
  { id: 'positions',      label: 'Positions',      active: false },
  { id: 'sections',       label: 'Sections',       active: false },
  { id: 'units',          label: 'Units',          active: false },
]

const SegmentTile = ({ label, active, onClick }) => (
  <button
    onClick={onClick}
    title={active ? 'Click to deactivate' : 'Click to activate'}
    className={`
      relative w-full rounded-xl px-4 py-5 text-center text-sm font-bold text-white
      shadow-md transition-all duration-150 active:scale-95 select-none
      ${active
        ? 'bg-gradient-to-br from-emerald-400 to-green-500 hover:from-emerald-500 hover:to-green-600 shadow-green-200 dark:shadow-green-900/30'
        : 'bg-gradient-to-br from-red-500 to-rose-600 hover:from-red-600 hover:to-rose-700 shadow-red-200 dark:shadow-red-900/30'
      }
    `}
  >
    {label}
    <span className={`
      absolute top-2 right-2 flex h-4 w-4 items-center justify-center rounded-full
      text-[9px] font-black
      ${active ? 'bg-white/30 text-white' : 'bg-white/20 text-white'}
    `}>
      {active ? '✓' : '×'}
    </span>
  </button>
)

const Configuration = () => {
  const [segments, setSegments] = useState(allSegments)

  const toggle = (id) =>
    setSegments((prev) =>
      prev.map((s) => (s.id === id ? { ...s, active: !s.active } : s))
    )

  const active   = segments.filter((s) => s.active)
  const inactive = segments.filter((s) => !s.active)

  return (
    <div className="space-y-4">
      {/* Breadcrumb */}
      <nav className="text-xs text-muted flex items-center gap-1.5">
        <a href="#/dashboard" className="text-brand-600 hover:underline dark:text-brand-400">home</a>
        <span>/</span>
        <span>Configuration</span>
      </nav>

      {/* Page card */}
      <div className="card overflow-hidden">
        {/* Card header bar */}
        <div className="bg-gradient-to-r from-[#1a4a72] to-[#1e5a8a] px-5 py-3">
          <h1 className="text-sm font-semibold text-white">Trans Atlantic World Incorporated</h1>
        </div>

        {/* Body */}
        <div className="p-5">
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">

            {/* Active Segments */}
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <span className="h-2.5 w-2.5 rounded-full bg-emerald-500" />
                <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">
                  Active Data Segments
                </h2>
                <span className="ml-auto badge bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                  {active.length}
                </span>
              </div>

              <div
                className="min-h-[280px] rounded-xl border-2 border-dashed border-slate-300 dark:border-slate-600 p-4
                            grid grid-cols-2 gap-3 content-start"
              >
                {active.length === 0 && (
                  <p className="col-span-2 flex items-center justify-center text-xs text-muted py-8">
                    No active segments. Click inactive tiles to activate.
                  </p>
                )}
                {active.map((s) => (
                  <SegmentTile key={s.id} label={s.label} active onClick={() => toggle(s.id)} />
                ))}
              </div>
            </div>

            {/* Inactive Segments */}
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <span className="h-2.5 w-2.5 rounded-full bg-red-500" />
                <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">
                  Inactive Data Segments
                </h2>
                <span className="ml-auto badge bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400">
                  {inactive.length}
                </span>
              </div>

              <div
                className="min-h-[280px] rounded-xl border-2 border-dashed border-slate-300 dark:border-slate-600 p-4
                            grid grid-cols-3 gap-3 content-start"
              >
                {inactive.length === 0 && (
                  <p className="col-span-3 flex items-center justify-center text-xs text-muted py-8">
                    All segments are active.
                  </p>
                )}
                {inactive.map((s) => (
                  <SegmentTile key={s.id} label={s.label} active={false} onClick={() => toggle(s.id)} />
                ))}
              </div>
            </div>
          </div>

          <p className="mt-4 text-xs text-muted">
            Click any tile to toggle its active/inactive status.
          </p>
        </div>
      </div>
    </div>
  )
}

export default Configuration
