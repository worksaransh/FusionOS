import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

/**
 * Kept separate from vite.config.ts (rather than adding a `test` block
 * there) so `vite build`/`vite dev` never pull in Vitest's type
 * augmentation — 02_TECH_STACK.md §5. Merges the real app config (React +
 * Tailwind plugins) rather than duplicating it, so tests transform JSX/CSS
 * exactly the way the app does. happy-dom is used as the DOM environment
 * instead of jsdom: same jsdom-shaped API for React Testing Library's
 * purposes, but a much smaller dependency tree, which also sidesteps a real
 * cross-filesystem npm install problem this sandbox hit with jsdom's
 * dependency tree (documented in README known-gaps — not a reason to avoid
 * jsdom on a normal machine, just what was reliable here).
 */
export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'happy-dom',
      globals: true,
      setupFiles: ['./src/test/setup.ts'],
      css: true,
      exclude: ['e2e/**', 'node_modules/**'],
    },
  }),
);
