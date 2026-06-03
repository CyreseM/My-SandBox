# PersonaX Central – Tailwind CSS BFF Template

A production-ready React admin template migrated from CoreUI Pro to **Tailwind CSS**.  
Retains all original features: i18n language switching, light/dark/auto theme, Redux state, and the custom navigation structure.

---

## Stack

| Layer       | Technology                          |
|-------------|-------------------------------------|
| UI          | React 19 + Tailwind CSS 3           |
| State       | Redux (same store shape as original)|
| Routing     | React Router v7 (HashRouter)        |
| i18n        | i18next + react-i18next             |
| Bundler     | Vite 6                              |
| Fonts       | DM Sans · JetBrains Mono (Google)   |

---

## Getting Started

```bash
npm install
npm run dev       # development server → http://localhost:5173
npm run build     # production build → dist/
npm run preview   # preview production build
```

---

## Features

### 🌍 Language Switching
Three locales ship out-of-the-box: **English**, **Español**, **Polski**.  
Translation files live in `public/locales/<lng>/translation.json`.  
Add a new language by:
1. Creating `public/locales/<code>/translation.json`
2. Adding `<code>` to `supportedLngs` in `src/i18n.js`
3. Adding a flag + label entry in `src/components/AppHeader.jsx`

### 🌗 Theme Modes
Three modes: **light**, **dark**, **auto** (follows OS preference).  
Powered by the `useTheme` hook in `src/hooks/useTheme.js`.  
Theme persisted in `localStorage` under key `personax-theme`.  
Tailwind `darkMode: 'class'` – the `dark` class is toggled on `<html>`.

### 📱 Responsive Sidebar
- Full sidebar on desktop (260px)
- Collapsible to icon-only mode (64px) via the chevron button
- Off-canvas drawer on mobile with backdrop overlay

### 📦 Redux Store
State shape (identical to original):
```js
{ sidebarShow: true, sidebarCollapsed: false, theme: 'light' }
```

---

## Project Structure

```
src/
├── components/         # Shared layout components
│   ├── AppHeader.jsx   #   Header with search, language, theme, user
│   ├── AppSidebar.jsx  #   Collapsible sidebar nav
│   ├── AppContent.jsx  #   Main content area + router outlet
│   ├── AppFooter.jsx   #   Footer
│   ├── AppBreadcrumb.jsx
│   └── NavIcon.jsx     #   Zero-dep inline SVG icon set
├── hooks/
│   └── useTheme.js     # Light / dark / auto theme hook
├── layout/
│   └── DefaultLayout.jsx
├── Pages/              # Feature pages (Country, TaxRegime…)
│   └── Forms/          #   Drawer forms (CountryForm…)
├── views/              # Remaining views (dashboard, auth pages…)
│   ├── dashboard/
│   └── pages/          #   login / register / 404 / 500
├── _nav.js             # Navigation config
├── routes.js           # Route definitions
├── store.js            # Redux store
├── i18n.js             # i18next config
├── App.jsx             # Root component
├── index.jsx           # React DOM entry
└── index.css           # Tailwind directives + design tokens
```

---

## Adding a New Page

1. Create `src/views/myfeature/MyFeature.jsx`
2. Add a route in `src/routes.js`
3. Add a nav entry in `src/_nav.js`
4. Add an SVG icon path in `src/components/NavIcon.jsx`

---

## Extending Tailwind

Edit `tailwind.config.js` to add custom colours, fonts, or breakpoints.  
The brand colour palette (`brand-*`) and dark surface tokens live there.

CSS custom properties (theme-aware colours) are declared in `src/index.css`
under `:root` and `.dark` so non-Tailwind CSS can reference them too.

---

## Licence

MIT
