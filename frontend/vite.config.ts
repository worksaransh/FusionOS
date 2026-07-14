import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// 02_TECH_STACK.md §5 — Vite + React + TypeScript, Tailwind v4 via first-party plugin.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // Routes frontend dev-server calls to the FusionOS.Api.Host per 03_SYSTEM_ARCHITECTURE.md.
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
