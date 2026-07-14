import { expect, test } from '@playwright/test';

/**
 * Smoke coverage for the one screen every unauthenticated visitor hits first
 * (07_SECURITY.md — RequireAuth redirects everything else here). No backend
 * call succeeds in this scaffold's environment, so these checks stay on the
 * client-only surface: routing guard + Zod validation + accessible labeling.
 * A follow-up (once the API is reachable in CI) adds a real login → redirect
 * to /core test using a seeded test user.
 */
test.describe('Login page', () => {
  test('unauthenticated visitors are redirected to /login', async ({ page }) => {
    await page.goto('/core');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('shows the sign-in form with accessible, labeled fields', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByRole('heading', { name: 'FusionOS' })).toBeVisible();
    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible();
  });

  test('rejects an empty submission with inline validation errors', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page.getByText('Enter a valid email')).toBeVisible();
    await expect(page.getByText('Password is required')).toBeVisible();
  });

  test('rejects a malformed email address', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel('Email').fill('not-an-email');
    await page.getByLabel('Password').fill('whatever');
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page.getByText('Enter a valid email')).toBeVisible();
  });
});
