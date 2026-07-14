import '@testing-library/jest-dom/vitest';

/**
 * Runs once before every test file (vitest.config.ts `test.setupFiles`).
 * Just wires in jest-dom's extra matchers (toBeInTheDocument, toHaveValue,
 * etc.) — everything else about the test DOM comes from happy-dom itself.
 */
