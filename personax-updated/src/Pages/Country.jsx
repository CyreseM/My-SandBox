import React, { useState } from 'react'
import { useTranslation } from 'react-i18next'
import NavIcon from '../components/NavIcon'
import CountryForm from './Forms/CountryForm'

const mockCountries = [
  { name: 'Algeria',            currency: 'Algerian dinar',         symbol: 'د.ج', status: 'Inactive' },
  { name: 'Anguilla',           currency: 'East Caribbean dollar',  symbol: '$',   status: 'Inactive' },
  { name: 'Antarctica',         currency: 'Australian dollar',      symbol: '$',   status: 'Inactive' },
  { name: 'Bahamas',            currency: 'Bahamian dollar',        symbol: '$',   status: 'Active'   },
  { name: 'Cameroon',           currency: 'Fijian dollar',          symbol: '$',   status: 'Active'   },
  { name: 'China',              currency: 'Chinese yuan',           symbol: '¥',   status: 'Inactive' },
  { name: 'Congo',              currency: 'CFA franc',              symbol: 'Fr',  status: 'Inactive' },
  { name: 'Dominican Republic', currency: 'Dominican peso',         symbol: '$',   status: 'Inactive' },
  { name: 'Estonia',            currency: 'Euro',                   symbol: '€',   status: 'Active'   },
  { name: 'Finland',            currency: 'Euro',                   symbol: '€',   status: 'Inactive' },
  { name: 'Ghana',              currency: 'Ghanaian cedi',          symbol: 'GHC', status: 'Active'   },
]

const StatusBadge = ({ status }) =>
  status === 'Active' ? (
    <span className="badge bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
      {status}
    </span>
  ) : (
    <span className="badge bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-400">
      {status}
    </span>
  )

const Country = () => {
  const { t } = useTranslation()
  const [search, setSearch]     = useState('')
  const [drawerOpen, setDrawer] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)

  const filtered = mockCountries.filter((c) =>
    c.name.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div className="space-y-4">
      <CountryForm visible={drawerOpen} setVisible={setDrawer} />

      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
          {t('Country Setup', 'Country Setup')}
        </h1>
        <p className="mt-1 text-sm text-muted">{t('country', 'Manage active countries and currencies')}</p>
      </div>

      <div className="card overflow-hidden">
        {/* Toolbar */}
        <div className="flex items-center justify-between gap-3 bg-slate-700 px-3 py-2">
          <div className="relative flex-1 max-w-xs">
            <NavIcon
              name="search"
              className="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-slate-400"
            />
            <input
              type="search"
              placeholder="Search Country"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-8 w-full rounded border-0 bg-slate-600 pl-8 pr-3 text-sm text-white
                         placeholder:text-slate-400 outline-none focus:ring-2 focus:ring-brand-500"
            />
          </div>
          <button
            onClick={() => setDrawer(true)}
            className="flex h-8 items-center gap-1.5 rounded bg-brand-600 px-3 text-sm font-semibold
                       text-white transition hover:bg-brand-700 active:scale-95"
          >
            <span className="text-lg leading-none">+</span>
            {t('new', 'Add')}
          </button>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="table-auto w-full">
            <thead>
              <tr className="border-b dark:border-slate-700">
                <th>{t('country', 'Name')}</th>
                <th>Currency</th>
                <th>Symbol</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((c, idx) => (
                <tr key={idx}>
                  <td className="font-medium">{c.name}</td>
                  <td>{c.currency}</td>
                  <td className="font-mono">{c.symbol}</td>
                  <td><StatusBadge status={c.status} /></td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={4} className="py-8 text-center text-muted">No countries found.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between border-t px-4 py-3 dark:border-slate-700">
          <div className="flex gap-1">
            {[1,2,3,4,5].map((n) => (
              <button
                key={n}
                onClick={() => setCurrentPage(n)}
                className={`h-7 w-7 rounded text-xs font-medium transition
                  ${n === currentPage
                    ? 'bg-brand-600 text-white'
                    : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-700'
                  }`}
              >
                {n}
              </button>
            ))}
          </div>
          <span className="text-xs text-muted">
            Page {currentPage} of 5 ({mockCountries.length} items)
          </span>
        </div>
      </div>
    </div>
  )
}

export default Country
