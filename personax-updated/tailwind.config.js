/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          50:  '#f0f4ff',
          100: '#dce8ff',
          200: '#b9d4ff',
          300: '#86b5ff',
          400: '#528aff',
          500: '#2c62f5',
          600: '#1a47eb',
          700: '#1535cc',
          800: '#172da6',
          900: '#192b82',
          950: '#111c55',
        },
        surface: {
          DEFAULT: '#ffffff',
          dark: '#0f172a',
        },
      },
      fontFamily: {
        sans: ['"DM Sans"', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'monospace'],
      },
    },
  },
  plugins: [],
}
