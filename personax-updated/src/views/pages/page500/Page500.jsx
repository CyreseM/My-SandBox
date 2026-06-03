import React from 'react'
import { Link } from 'react-router-dom'

const Page500 = () => (
  <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 dark:bg-slate-950 text-center px-4">
    <p className="text-8xl font-black text-red-500 opacity-20">500</p>
    <h1 className="mt-4 text-2xl font-bold text-slate-900 dark:text-white">Server Error</h1>
    <p className="mt-2 text-sm text-muted">Something went wrong on our end. Please try again.</p>
    <Link to="/" className="btn-primary mt-6 inline-flex">Go Home</Link>
  </div>
)

export default Page500
