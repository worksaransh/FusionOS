import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

// PWA app-shell cache (see public/sw.js) — lets the app be installed from the
// browser and load without a network connection; it never caches /api/* data.
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(() => {
      // Non-fatal — the app works fine without the service worker, it just
      // loses the "installable/works offline" PWA behavior.
    })
  })
}
