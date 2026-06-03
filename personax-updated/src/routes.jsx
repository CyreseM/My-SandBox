import React from 'react'

const Dashboard      = React.lazy(() => import('./views/dashboard/Dashboard'))
const Country        = React.lazy(() => import('./Pages/Country'))
const TaxRegime      = React.lazy(() => import('./Pages/TaxRegime'))
const Configuration  = React.lazy(() => import('./Pages/Configuration'))
const EmployeeStatus = React.lazy(() => import('./Pages/EmployeeStatus'))
const BankBranches   = React.lazy(() => import('./Pages/BankBranches'))

const routes = [
  { path: '/',               exact: true, name: 'Home' },
  { path: '/dashboard',      name: 'Dashboard',       element: Dashboard },
  { path: '/configuration',  name: 'Configuration',   element: Configuration },
  { path: '/country',        name: 'Country Setup',   element: Country },
  { path: '/taxregime',      name: 'Tax Regime',      element: TaxRegime },
  { path: '/employee-status',name: 'Employee Status', element: EmployeeStatus },
  { path: '/bank-branches',  name: 'Bank & Branches', element: BankBranches },
  { path: '/products',       name: 'Products',        element: Placeholder('Products') },
  { path: '/licensing',      name: 'Licensing',       element: Placeholder('Licensing') },
  { path: '/pricing',        name: 'App Pricing',     element: Placeholder('App Pricing') },
  { path: '/subscriptions',  name: 'Subscriptions',   element: Placeholder('Subscriptions') },
]

function Placeholder(title) {
  return function PlaceholderPage() {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{title}</h1>
        <div className="card flex h-48 items-center justify-center text-muted">
          <p className="text-sm">🚧 This page is under construction.</p>
        </div>
      </div>
    )
  }
}

export default routes
