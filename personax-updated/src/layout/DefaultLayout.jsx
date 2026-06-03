import React from 'react'
import AppSidebar from '../components/AppSidebar'
import AppHeader from '../components/AppHeader'
import AppContent from '../components/AppContent'
import AppFooter from '../components/AppFooter'

const DefaultLayout = () => {
  return (
    <div className="flex min-h-screen">
      <AppSidebar />
      <div className="flex flex-1 flex-col min-w-0">
        <AppHeader />
        <AppContent />
        <AppFooter />
      </div>
    </div>
  )
}

export default DefaultLayout
