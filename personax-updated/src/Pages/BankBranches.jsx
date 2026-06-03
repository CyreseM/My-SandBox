import React, { useState } from 'react'

/* ── Mock data ──────────────────────────────────────────────────────────── */
const banksData = {
  'BANK OF AFRICA': [
    { code: '210117', name: 'BANK OF AFRICA- DANSOMAN BRANCH' },
    { code: '210601', name: 'BANK OF AFRICA -KUMASI AMAKOM BRANCH' },
    { code: '210604', name: 'BANK OF AFRICA- SUAME BRANCH' },
    { code: '210401', name: 'BANK OF AFRICA -TAKORADI BRANCH' },
    { code: '210119', name: 'BANK OF AFRICA-ABOSSEY OKAI BRANCH' },
    { code: '210104', name: 'BANK OF AFRICA-ACCRA CENTRAL BRANCH' },
    { code: '210125', name: 'BANK OF AFRICA-AIRPORT CITY BRANCH' },
    { code: '210901', name: 'BANK OF AFRICA-BOLGATANGA BRANCH' },
    { code: '210111', name: 'BANK OF AFRICA-EAST LEGON BRANCH' },
    { code: '210101', name: 'BANK OF AFRICA-FARRAR AVE BRANCH' },
    { code: '210602', name: 'BANK OF AFRICA-KUMASI -ADUM BRANCH' },
    { code: '210112', name: 'BANK OF AFRICA-KWASHIEMAN BRANCH' },
    { code: '210103', name: 'BANK OF AFRICA-MAAMOBI BRANCH' },
    { code: '210105', name: 'BANK OF AFRICA-OKAISHIE BRANCH' },
    { code: '210106', name: 'BANK OF AFRICA-RING ROAD EAST BRANCH' },
    { code: '210107', name: 'BANK OF AFRICA-TEMA BRANCH' },
    { code: '210108', name: 'BANK OF AFRICA-TESANO BRANCH' },
    { code: '210109', name: 'BANK OF AFRICA-ADABRAKA BRANCH' },
    { code: '210110', name: 'BANK OF AFRICA-ACHIMOTA BRANCH' },
    { code: '210113', name: 'BANK OF AFRICA-LABONE BRANCH' },
    { code: '210114', name: 'BANK OF AFRICA-SPINTEX BRANCH' },
    { code: '210115', name: 'BANK OF AFRICA-ASHAIMAN BRANCH' },
    { code: '210116', name: 'BANK OF AFRICA-MADINA BRANCH' },
    { code: '210118', name: 'BANK OF AFRICA-KANESHIE BRANCH' },
    { code: '210120', name: 'BANK OF AFRICA-KOFORIDUA BRANCH' },
    { code: '210121', name: 'BANK OF AFRICA-SUNYANI BRANCH' },
    { code: '210122', name: 'BANK OF AFRICA-TAMALE BRANCH' },
    { code: '210123', name: 'BANK OF AFRICA-HO BRANCH' },
  ],
  'GCB BANK': [
    { code: '300101', name: 'GCB BANK-ACCRA CENTRAL BRANCH' },
    { code: '300102', name: 'GCB BANK-TEMA BRANCH' },
    { code: '300103', name: 'GCB BANK-KUMASI BRANCH' },
    { code: '300104', name: 'GCB BANK-TAKORADI BRANCH' },
    { code: '300105', name: 'GCB BANK-TAMALE BRANCH' },
    { code: '300106', name: 'GCB BANK-CAPE COAST BRANCH' },
    { code: '300107', name: 'GCB BANK-HO BRANCH' },
    { code: '300108', name: 'GCB BANK-KOFORIDUA BRANCH' },
    { code: '300109', name: 'GCB BANK-SUNYANI BRANCH' },
    { code: '300110', name: 'GCB BANK-BOLGATANGA BRANCH' },
  ],
  'ECOBANK GHANA': [
    { code: '400101', name: 'ECOBANK-ACCRA HIGH STREET BRANCH' },
    { code: '400102', name: 'ECOBANK-TEMA COMMUNITY 1 BRANCH' },
    { code: '400103', name: 'ECOBANK-KUMASI BRANCH' },
    { code: '400104', name: 'ECOBANK-TAKORADI BRANCH' },
    { code: '400105', name: 'ECOBANK-MADINA BRANCH' },
    { code: '400106', name: 'ECOBANK-SPINTEX BRANCH' },
    { code: '400107', name: 'ECOBANK-ACHIMOTA BRANCH' },
  ],
  'STANBIC BANK': [
    { code: '500101', name: 'STANBIC-ACCRA MAIN BRANCH' },
    { code: '500102', name: 'STANBIC-AIRPORT CITY BRANCH' },
    { code: '500103', name: 'STANBIC-KUMASI BRANCH' },
    { code: '500104', name: 'STANBIC-TAKORADI BRANCH' },
    { code: '500105', name: 'STANBIC-EAST LEGON BRANCH' },
  ],
}

const PER_PAGE = 13

const BankBranches = () => {
  const banks = Object.keys(banksData)
  const [selectedBank, setSelectedBank] = useState(banks[0])
  const [applied, setApplied]           = useState(banks[0])
  const [page, setPage]                 = useState(1)

  const branches = banksData[applied] || []
  const totalPages = Math.max(1, Math.ceil(branches.length / PER_PAGE))
  const paged = branches.slice((page - 1) * PER_PAGE, page * PER_PAGE)

  const handleSearch = () => { setApplied(selectedBank); setPage(1) }
  const handleReset  = () => { setSelectedBank(banks[0]); setApplied(banks[0]); setPage(1) }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Bank &amp; Branches</h1>
        <p className="mt-1 text-sm text-muted">View all branches per bank</p>
      </div>

      <div className="card overflow-hidden">
        {/* Header bar */}
        <div className="bg-gradient-to-r from-[#1a4a72] to-[#1e5a8a] px-5 py-2.5">
          <h2 className="text-sm font-semibold text-white">Bank &amp; Branches</h2>
        </div>

        {/* Filter row */}
        <div className="flex items-end gap-3 px-5 pt-4 pb-3 border-b dark:border-slate-700">
          <div>
            <label className="form-label">Select Bank</label>
            <div className="flex items-center gap-2">
              <select
                value={selectedBank}
                onChange={(e) => setSelectedBank(e.target.value)}
                className="input text-sm h-9 w-56"
              >
                {banks.map((b) => <option key={b} value={b}>{b}</option>)}
              </select>
              {/* Search button */}
              <button
                onClick={handleSearch}
                className="flex h-9 w-9 items-center justify-center rounded-lg bg-brand-600 text-white
                           transition hover:bg-brand-700 active:scale-95 flex-shrink-0"
                title="Search"
              >
                <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/>
                </svg>
              </button>
              {/* Reset button */}
              <button
                onClick={handleReset}
                className="flex h-9 w-9 items-center justify-center rounded-lg border bg-white text-slate-500
                           transition hover:bg-slate-50 hover:text-slate-700 active:scale-95 flex-shrink-0
                           dark:bg-slate-800 dark:text-slate-400 dark:hover:bg-slate-700 dark:border-slate-600"
                title="Reset"
              >
                <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 5V1L7 6l5 5V7c3.31 0 6 2.69 6 6s-2.69 6-6 6-6-2.69-6-6H4c0 4.42 3.58 8 8 8s8-3.58 8-8-3.58-8-8-8z"/>
                </svg>
              </button>
            </div>
          </div>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="table-auto w-full">
            <thead>
              <tr className="border-b dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50">
                <th className="w-1/3">Branch Code</th>
                <th>Branch Name</th>
              </tr>
            </thead>
            <tbody>
              {paged.map((branch) => (
                <tr key={branch.code} className="cursor-pointer" onClick={() => {}}>
                  <td className="font-mono text-sm font-medium text-slate-700 dark:text-slate-300">
                    {branch.code}
                  </td>
                  <td className="text-sm">{branch.name}</td>
                </tr>
              ))}
              {paged.length === 0 && (
                <tr>
                  <td colSpan={2} className="py-10 text-center text-muted">No branches found.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between border-t px-4 py-3 dark:border-slate-700">
          <div className="flex items-center gap-1">
            <button
              onClick={() => setPage(1)} disabled={page === 1}
              className="h-7 w-7 rounded text-xs font-medium text-slate-600 hover:bg-slate-100
                         disabled:opacity-40 dark:text-slate-400 dark:hover:bg-slate-700"
            >«</button>
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
              className="h-7 w-7 rounded text-xs font-medium text-slate-600 hover:bg-slate-100
                         disabled:opacity-40 dark:text-slate-400 dark:hover:bg-slate-700"
            >‹</button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
              <button
                key={n}
                onClick={() => setPage(n)}
                className={`h-7 min-w-[28px] rounded px-1.5 text-xs font-medium transition
                  ${n === page ? 'bg-brand-600 text-white' : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-700'}`}
              >{n}</button>
            ))}
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}
              className="h-7 w-7 rounded text-xs font-medium text-slate-600 hover:bg-slate-100
                         disabled:opacity-40 dark:text-slate-400 dark:hover:bg-slate-700"
            >›</button>
            <button
              onClick={() => setPage(totalPages)} disabled={page === totalPages}
              className="h-7 w-7 rounded text-xs font-medium text-slate-600 hover:bg-slate-100
                         disabled:opacity-40 dark:text-slate-400 dark:hover:bg-slate-700"
            >»</button>
          </div>
          <span className="text-xs text-muted">
            Page {page} of {totalPages} ({branches.length} items)
          </span>
        </div>
      </div>
    </div>
  )
}

export default BankBranches
