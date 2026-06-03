import React, { useState } from 'react'

/* ── Add / Edit Form Modal ──────────────────────────────────────────────── */
const EmployeeStatusForm = ({ visible, setVisible, onSave, editData }) => {
  const empty = { code: '', name: '', payrollAction: '', actionType: '', percentage: '0.00', note: '' }
  const [form, setForm] = useState(editData || empty)

  React.useEffect(() => { setForm(editData || empty) }, [editData, visible])

  const set = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }))
  const reset = () => setForm(editData || empty)

  const handleSave = () => {
    if (!form.code || !form.name || !form.payrollAction) return
    onSave(form)
    setVisible(false)
  }

  React.useEffect(() => {
    const h = (e) => { if (e.key === 'Escape') setVisible(false) }
    document.addEventListener('keydown', h)
    return () => document.removeEventListener('keydown', h)
  }, [setVisible])

  return (
    <>
      {visible && (
        <div className="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm" onClick={() => setVisible(false)} />
      )}
      <div className={`
        fixed inset-y-0 right-0 z-50 flex w-full max-w-lg flex-col bg-white shadow-2xl
        transition-transform duration-300 dark:bg-slate-900
        ${visible ? 'translate-x-0' : 'translate-x-full'}
      `}>
        {/* Header */}
        <div className="bg-gradient-to-r from-[#1a4a72] to-[#1e5a8a] px-5 py-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-white">
            {editData ? 'Edit Employee Status' : 'Add Employee Status'}
          </h2>
          <button
            onClick={() => setVisible(false)}
            className="flex h-7 w-7 items-center justify-center rounded-md text-white/70 hover:bg-white/10 hover:text-white"
            aria-label="Close"
          >
            <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          {/* Row 1: Code + Name */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="form-label">Code <span className="text-red-500">*</span></label>
              <input
                type="text"
                placeholder="Enter Code"
                value={form.code}
                onChange={set('code')}
                className="input text-sm h-9"
              />
            </div>
            <div className="col-span-2">
              <label className="form-label">Name <span className="text-red-500">*</span></label>
              <input
                type="text"
                placeholder="Enter Name"
                value={form.name}
                onChange={set('name')}
                className="input text-sm h-9"
              />
            </div>
          </div>

          {/* Row 2: Payroll Action + Action Type + Percentage */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="form-label">Payroll Action <span className="text-red-500">*</span></label>
              <select value={form.payrollAction} onChange={set('payrollAction')} className="input text-sm h-9">
                <option value="">Select Payroll Action</option>
                <option value="addition">Addition</option>
                <option value="deduction">Deduction</option>
                <option value="neutral">Neutral</option>
              </select>
            </div>
            <div>
              <label className="form-label">Action Type</label>
              <select value={form.actionType} onChange={set('actionType')} className="input text-sm h-9">
                <option value="">Select Action Type</option>
                <option value="fixed">Fixed</option>
                <option value="percentage">Percentage</option>
                <option value="formula">Formula</option>
              </select>
            </div>
            <div>
              <label className="form-label">Percentage</label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={form.percentage}
                onChange={set('percentage')}
                className="input text-sm h-9 text-right"
              />
            </div>
          </div>

          {/* Note */}
          <div>
            <label className="form-label">Note</label>
            <textarea
              placeholder="Enter Note"
              value={form.note}
              onChange={set('note')}
              rows={4}
              className="input text-sm resize-none"
            />
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between border-t px-5 py-3 dark:border-slate-700 bg-slate-50 dark:bg-slate-900/50">
          <p className="text-xs text-red-500">All fields marked with asterisk are required(*)</p>
          <div className="flex gap-2">
            <button onClick={() => setVisible(false)} className="btn-secondary text-sm h-9 px-4 flex items-center gap-1.5">
              <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor"><path d="M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>
              Cancel
            </button>
            <button onClick={reset} className="inline-flex items-center gap-1.5 rounded-lg bg-amber-500 px-4 h-9 text-sm font-semibold text-white transition hover:bg-amber-600 active:scale-95">
              <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor"><path d="M12 5V1L7 6l5 5V7c3.31 0 6 2.69 6 6s-2.69 6-6 6-6-2.69-6-6H4c0 4.42 3.58 8 8 8s8-3.58 8-8-3.58-8-8-8z"/></svg>
              Reset
            </button>
            <button onClick={handleSave} className="inline-flex items-center gap-1.5 rounded-lg bg-brand-600 px-4 h-9 text-sm font-semibold text-white transition hover:bg-brand-700 active:scale-95">
              <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor"><path d="M17 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V7l-4-4zm-5 16c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3zm3-10H5V5h10v4z"/></svg>
              Save
            </button>
          </div>
        </div>
      </div>
    </>
  )
}

/* ── Mock data ──────────────────────────────────────────────────────────── */
const mockData = [
  { code: 'ES001', name: 'Active',       payrollAction: 'addition',  actionType: 'fixed',      percentage: '0.00', note: '' },
  { code: 'ES002', name: 'On Leave',     payrollAction: 'deduction', actionType: 'percentage', percentage: '5.00', note: 'Unpaid leave deduction' },
  { code: 'ES003', name: 'Probation',    payrollAction: 'neutral',   actionType: 'fixed',      percentage: '0.00', note: 'Trial period' },
  { code: 'ES004', name: 'Suspended',    payrollAction: 'deduction', actionType: 'percentage', percentage: '100.00', note: '' },
  { code: 'ES005', name: 'Terminated',   payrollAction: 'neutral',   actionType: '',           percentage: '0.00', note: '' },
]

/* ── Main page ──────────────────────────────────────────────────────────── */
const EmployeeStatus = () => {
  const [rows, setRows] = useState(mockData)
  const [search, setSearch] = useState('')
  const [drawerOpen, setDrawer] = useState(false)
  const [editData, setEditData] = useState(null)
  const [page, setPage] = useState(1)
  const perPage = 10

  const filtered = rows.filter(
    (r) =>
      r.code.toLowerCase().includes(search.toLowerCase()) ||
      r.name.toLowerCase().includes(search.toLowerCase())
  )
  const totalPages = Math.max(1, Math.ceil(filtered.length / perPage))
  const paged = filtered.slice((page - 1) * perPage, page * perPage)

  const openAdd = () => { setEditData(null); setDrawer(true) }
  const openEdit = (row) => { setEditData(row); setDrawer(true) }
  const handleSave = (form) => {
    setRows((prev) => {
      const idx = prev.findIndex((r) => r.code === form.code)
      if (idx >= 0) { const n = [...prev]; n[idx] = form; return n }
      return [...prev, form]
    })
  }
  const handleDelete = (code) => setRows((prev) => prev.filter((r) => r.code !== code))

  return (
    <div className="space-y-4">
      <EmployeeStatusForm
        visible={drawerOpen}
        setVisible={setDrawer}
        onSave={handleSave}
        editData={editData}
      />

      {/* Page heading */}
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Employee Status</h1>
        <p className="mt-1 text-sm text-muted">Manage payroll-linked employee statuses</p>
      </div>

      <div className="card overflow-hidden">
        {/* Toolbar */}
        <div className="flex items-center justify-between gap-3 bg-gradient-to-r from-[#1a4a72] to-[#1e5a8a] px-4 py-2.5">
          <div className="relative max-w-xs flex-1">
            <svg className="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-slate-300" viewBox="0 0 24 24" fill="currentColor">
              <path d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/>
            </svg>
            <input
              type="search"
              placeholder="Search..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1) }}
              className="h-8 w-full rounded border-0 bg-white/10 pl-8 pr-3 text-sm text-white
                         placeholder:text-slate-300 outline-none focus:ring-2 focus:ring-white/30"
            />
          </div>
          <button
            onClick={openAdd}
            className="flex h-8 items-center gap-1.5 rounded bg-white/15 px-3 text-sm font-semibold
                       text-white transition hover:bg-white/25 active:scale-95 border border-white/20"
          >
            <span className="text-lg leading-none">+</span> Add
          </button>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="table-auto w-full">
            <thead>
              <tr className="border-b dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50">
                <th>Code</th>
                <th>Name</th>
                <th>Payroll Action</th>
                <th>Action Type</th>
                <th className="text-right">Percentage</th>
                <th>Note</th>
                <th className="text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              {paged.map((row) => (
                <tr key={row.code}>
                  <td className="font-mono text-xs font-semibold text-brand-600 dark:text-brand-400">{row.code}</td>
                  <td className="font-medium">{row.name}</td>
                  <td>
                    <span className={`badge capitalize
                      ${row.payrollAction === 'addition'  ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400' : ''}
                      ${row.payrollAction === 'deduction' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' : ''}
                      ${row.payrollAction === 'neutral'   ? 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-400' : ''}
                    `}>{row.payrollAction || '—'}</span>
                  </td>
                  <td className="capitalize">{row.actionType || '—'}</td>
                  <td className="text-right font-mono text-sm">{parseFloat(row.percentage).toFixed(2)}%</td>
                  <td className="max-w-[160px] truncate text-muted">{row.note || '—'}</td>
                  <td>
                    <div className="flex items-center justify-center gap-1">
                      <button
                        onClick={() => openEdit(row)}
                        className="flex h-7 w-7 items-center justify-center rounded-md text-brand-600 transition
                                   hover:bg-brand-50 dark:text-brand-400 dark:hover:bg-brand-900/20"
                        title="Edit"
                      >
                        <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor">
                          <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/>
                        </svg>
                      </button>
                      <button
                        onClick={() => handleDelete(row.code)}
                        className="flex h-7 w-7 items-center justify-center rounded-md text-red-500 transition
                                   hover:bg-red-50 dark:hover:bg-red-900/20"
                        title="Delete"
                      >
                        <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor">
                          <path d="M6 19a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/>
                        </svg>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {paged.length === 0 && (
                <tr><td colSpan={7} className="py-10 text-center text-muted">No records found.</td></tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between border-t px-4 py-3 dark:border-slate-700">
          <div className="flex items-center gap-1">
            <button onClick={() => setPage(1)} disabled={page === 1} className="pagination-btn">«</button>
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="pagination-btn">‹</button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
              <button
                key={n}
                onClick={() => setPage(n)}
                className={`h-7 min-w-[28px] rounded px-1.5 text-xs font-medium transition
                  ${n === page ? 'bg-brand-600 text-white' : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-700'}`}
              >{n}</button>
            ))}
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="pagination-btn">›</button>
            <button onClick={() => setPage(totalPages)} disabled={page === totalPages} className="pagination-btn">»</button>
          </div>
          <span className="text-xs text-muted">Page {page} of {totalPages} ({filtered.length} items)</span>
        </div>
      </div>
    </div>
  )
}

export default EmployeeStatus
