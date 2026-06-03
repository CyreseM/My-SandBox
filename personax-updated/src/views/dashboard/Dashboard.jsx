import React from 'react'
import { useTranslation } from 'react-i18next'

const StatCard = ({ label, value, change, changeType }) => (
  <div className="card p-5">
    <p className="text-xs font-semibold uppercase tracking-wide text-muted mb-1">{label}</p>
    <p className="text-2xl font-bold text-slate-900 dark:text-white">{value}</p>
    {change && (
      <p className={`mt-1 text-xs font-medium ${changeType === 'up' ? 'text-emerald-600' : 'text-red-500'}`}>
        {changeType === 'up' ? '▲' : '▼'} {change}
      </p>
    )}
  </div>
)

const Dashboard = () => {
  const { t } = useTranslation()

  const stats = [
    { label: t('newClients', 'New Clients'), value: '9,652',    change: '12.4% this month', changeType: 'up' },
    { label: t('recurring',  'Recurring'),   value: '24,180',   change: '3.1% this month',  changeType: 'up' },
    { label: t('pageviews',  'Pageviews'),   value: '78,623',   change: '2.5% this month',  changeType: 'down' },
    { label: t('sessions',   'Sessions'),    value: '129,940',  change: '8.7% this month',  changeType: 'up' },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
          {t('dashboard', 'Dashboard')}
        </h1>
        <p className="mt-1 text-sm text-muted">{t('trafficAndSales', 'Traffic & Sales')}</p>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {stats.map((s) => <StatCard key={s.label} {...s} />)}
      </div>

      {/* Placeholder chart area */}
      <div className="card p-6">
        <h2 className="mb-4 text-sm font-semibold text-slate-700 dark:text-slate-300">
          {t('traffic', 'Traffic')}
        </h2>
        <div className="flex h-48 items-end gap-1">
          {[40, 65, 52, 80, 72, 91, 68, 55, 78, 62, 88, 74].map((h, i) => (
            <div
              key={i}
              className="flex-1 rounded-t bg-brand-500/80 transition-all hover:bg-brand-600"
              style={{ height: `${h}%` }}
            />
          ))}
        </div>
        <div className="mt-2 flex justify-between text-[10px] text-muted">
          {['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'].map((m) => (
            <span key={m}>{m}</span>
          ))}
        </div>
      </div>

      {/* Recent activity */}
      <div className="card overflow-hidden">
        <div className="px-5 py-4 border-b dark:border-slate-700">
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-300">
            {t('activity', 'Recent Activity')}
          </h2>
        </div>
        <table className="table-auto w-full">
          <thead>
            <tr className="border-b dark:border-slate-700">
              <th>{t('user', 'User')}</th>
              <th>{t('activity', 'Activity')}</th>
              <th>{t('date', { date: new Date(), defaultValue: 'Date' })}</th>
            </tr>
          </thead>
          <tbody>
            {[
              { user: 'Alice Johnson', activity: 'Created country profile', date: 'Today 09:14' },
              { user: 'Bob Smith',     activity: 'Updated tax regime',      date: 'Today 08:52' },
              { user: 'Carol White',   activity: 'Added new product',       date: 'Yesterday'   },
              { user: 'David Lee',     activity: 'Modified subscription',   date: 'Yesterday'   },
            ].map((row) => (
              <tr key={row.user}>
                <td>
                  <div className="flex items-center gap-2">
                    <div className="h-7 w-7 rounded-full bg-brand-100 dark:bg-brand-900/40 flex items-center justify-center text-xs font-bold text-brand-700 dark:text-brand-300">
                      {row.user[0]}
                    </div>
                    {row.user}
                  </div>
                </td>
                <td>{row.activity}</td>
                <td className="text-muted">{row.date}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default Dashboard
