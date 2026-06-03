export const nav = [
  { type: 'item', key: 'dashboard',      label: 'Dashboard',        to: '/dashboard',      icon: 'speedometer', badge: { color: 'info', text: 'NEW' } },

  { type: 'title', label: 'Configuration' },
  { type: 'item', key: 'configuration',  label: 'Configuration',    to: '/configuration',  icon: 'settings' },

  { type: 'title', label: 'Country' },
  { type: 'item', key: 'country',        label: 'Country Setup',    to: '/country',        icon: 'globe' },
  { type: 'item', key: 'taxregime',      label: 'Tax Regime',       to: '/taxregime',      icon: 'laptop' },

  { type: 'title', label: 'HR Setup' },
  { type: 'item', key: 'empstatus',      label: 'Employee Status',  to: '/employee-status', icon: 'people' },
  { type: 'item', key: 'bankbranches',   label: 'Bank & Branches',  to: '/bank-branches',  icon: 'dollar' },

  { type: 'title', label: 'Product and License' },
  { type: 'item', key: 'products',       label: 'Products',         to: '/products',       icon: 'cart' },
  { type: 'item', key: 'licensing',      label: 'Licensing',        to: '/licensing',      icon: 'key' },
  { type: 'item', key: 'pricing',        label: 'App Pricing',      to: '/pricing',        icon: 'dollar' },

  { type: 'title', label: 'Subscriptions' },
  { type: 'item', key: 'subscriptions',  label: 'Subscriptions',    to: '/subscriptions',  icon: 'people', badge: { color: 'danger', text: 'PRO' } },
]

export default nav
