import React from 'react'
import lightLogo from '../../assets/images/logo-light.svg'
import { ChevronDown, Sun } from 'lucide-react';
const Header = () => {
  return (
    <header className="px-8 py-4 flex justify-between items-center">
      <a href="/" className="cursor-pointer">
        <img src={lightLogo} width={80} alt="logo" />
      </a>
      <div className="grid grid-cols-4 gap-4">
        <p className="headerNav">Home</p>
        <p className="headerNav">Features</p>
        <p className="headerNav flex">
          Pages&nbsp;
          <ChevronDown className="mt-auto" size={16} />
        </p>
        <p className="headerNav">Support</p>
      </div>

      <div className="flex items-center space-x-4">
        <p className="cursor-pointer text-white">
          <Sun />
        </p>
        <p className="text-white">Sign In</p>
        <button className="bg-blue-600 text-white px-4 py-2 rounded-full hover:bg-blue-700 transition">
          Sign Up
        </button>
      </div>
    </header>
  );
}

export default Header
