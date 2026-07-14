import { defineConfig, devices } from '@playwright/test';

/**
 * E2E scaffold (05_MODULE_ROADMAP.md / audit Testing gap — the app had zero
 * end-to-end coverage; Vitest+RTL above covers components in isolation, this
 * covers real user flows through a running browser against the real Vite
 * dev server). `webServer` starts `npm run dev` itself and waits for it to
 * respond, so `npx playwright test` is a single command with no separate
 * "start the app first" step — CI can run it exactly the same way.
 *
 * Scoped to Chromium only for now (not the full desktop-browser matrix):
 * one browser is enough to catch real regressions in the flows below, and
 * keeps the browser-binary download this needs down to a single engine.
 * Add `devices['Desktop Firefox']` / `devices['Desktop Safari']` projects
 * once there's an actual cross-browser bug to guard against.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 30_000,
  },
});
