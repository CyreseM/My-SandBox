import React, { useState } from 'react'
import { useTranslation } from 'react-i18next'

const countries = [
  { name: 'Ghana',   capital: 'Accra',     continent: 'Africa',  currency: 'Ghanaian cedi', code: '+233', population: '33,475,870',   flag: 'https://flagcdn.com/w80/gh.png' },
  { name: 'Nigeria', capital: 'Abuja',     continent: 'Africa',  currency: 'Nigerian naira', code: '+234', population: '223,804,632', flag: 'https://flagcdn.com/w80/ng.png' },
  { name: 'Kenya',   capital: 'Nairobi',   continent: 'Africa',  currency: 'Kenyan shilling', code: '+254', population: '55,864,655', flag: 'https://flagcdn.com/w80/ke.png' },
  { name: 'Estonia', capital: 'Tallinn',   continent: 'Europe',  currency: 'Euro',            code: '+372', population: '1,365,884',  flag: 'https://flagcdn.com/w80/ee.png' },
  { name: 'Bahamas', capital: 'Nassau',    continent: 'Americas',currency: 'Bahamian dollar', code: '+1',   population: '412,623',    flag: 'https://flagcdn.com/w80/bs.png' },
]

const tabs = ['General', 'VAT / GST', 'Withholding Tax', 'Stamp Duty']

const TaxRegime = () => {
  const { t } = useTranslation()
  const [selected, setSelected] = useState(countries[0])
  const [activeTab, setActiveTab] = useState(0)

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
          {t('Tax Regime', 'Tax Regime')}
        </h1>
        <p className="mt-1 text-sm text-muted">Configure tax rules per country</p>
      </div>

      <div className="card overflow-hidden">
        {/* Country selector toolbar */}
        <div className="flex items-center bg-slate-700 px-3 py-2">
          <select
            value={selected.name}
            onChange={(e) => setSelected(countries.find((c) => c.name === e.target.value))}
            className="h-8 rounded border-0 bg-slate-600 px-2 text-sm text-white
                       outline-none focus:ring-2 focus:ring-brand-500"
          >
            {countries.map((c) => (
              <option key={c.name} value={c.name}>{c.name}</option>
            ))}
          </select>
        </div>

        <div className="p-4">
          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            {/* Country info panel */}
            <div className="space-y-3">
              {/* Flag */}
              <div className="overflow-hidden rounded-lg border-2 border-emerald-500 w-24">
                <img src={selected.flag} alt={selected.name} className="w-full object-cover" />
              </div>

              {[
                { label: 'Country',    value: selected.name      },
                { label: 'Capital',    value: selected.capital   },
                { label: 'Continent',  value: selected.continent },
                { label: 'Currency',   value: selected.currency  },
              ].map(({ label, value }) => (
                <div key={label}>
                  <label className="mb-0.5 block text-xs font-medium text-slate-600 dark:text-slate-400">
                    {label}
                  </label>
                  <input
                    type="text"
                    disabled
                    value={value}
                    readOnly
                    className="input h-7 text-sm opacity-70 cursor-not-allowed"
                  />
                </div>
              ))}

              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="mb-0.5 block text-right text-xs font-medium text-slate-600 dark:text-slate-400">
                    Code
                  </label>
                  <input
                    type="text"
                    disabled
                    value={selected.code}
                    readOnly
                    className="input h-7 text-sm text-right opacity-70 cursor-not-allowed"
                  />
                </div>
                <div>
                  <label className="mb-0.5 block text-right text-xs font-medium text-slate-600 dark:text-slate-400">
                    Population
                  </label>
                  <input
                    type="text"
                    disabled
                    value={selected.population}
                    readOnly
                    className="input h-7 text-sm text-right opacity-70 cursor-not-allowed"
                  />
                </div>
              </div>
            </div>

            {/* Tax tabs */}
            <div className="md:col-span-2">
              <div className="border-b dark:border-slate-700">
                <nav className="flex gap-1 overflow-x-auto">
                  {tabs.map((tab, idx) => (
                    <button
                      key={tab}
                      onClick={() => setActiveTab(idx)}
                      className={`flex-shrink-0 border-b-2 px-4 py-2 text-sm font-medium transition
                        ${activeTab === idx
                          ? 'border-brand-600 text-brand-600 dark:text-brand-400'
                          : 'border-transparent text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'
                        }`}
                    >
                      {tab}
                    </button>
                  ))}
                </nav>
              </div>

              <div className="pt-4">
                {activeTab === 0 && (
                  <div className="space-y-3 text-sm text-muted">
                    <p>General tax configuration for <strong className="text-slate-900 dark:text-white">{selected.name}</strong>.</p>
                    <div className="grid grid-cols-2 gap-3">
                      {[
                        { label: 'Tax Year Start', placeholder: '01 January' },
                        { label: 'Tax Year End',   placeholder: '31 December' },
                        { label: 'Filing Deadline', placeholder: '30 April'  },
                        { label: 'Tax Authority',   placeholder: 'GRA'       },
                      ].map(({ label, placeholder }) => (
                        <div key={label}>
                          <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">{label}</label>
                          <input type="text" placeholder={placeholder} className="input h-8 text-sm" />
                        </div>
                      ))}
                    </div>
                  </div>
                )}
                {activeTab !== 0 && (
                  <p className="py-8 text-center text-sm text-muted">
                    {tabs[activeTab]} configuration for {selected.name} — coming soon.
                  </p>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default TaxRegime
