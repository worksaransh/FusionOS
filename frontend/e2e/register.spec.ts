import { expect, test } from '@playwright/test';

/**
 * Coverage for the public sign-up screen (RegisterPage.tsx — Phase H5). Same
 * client-only scoping rationale as login.spec.ts: no backend is reachable in
 * this environment, so these stay on routing/Zod/accessible-labeling checks
 * rather than a real POST /core/companies + /core/auth/register round trip.
 */
test.describe('Register page', () => {
  test('shows the sign-up form with accessible, labeled fields', async ({ page }) => {
    await page.goto('/register');

    await expect(page.getByRole('heading', { name: 'FusionOS' })).toBeVisible();
    await expect(page.getByLabel('Full name')).toBeVisible();
    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password', { exact: true })).toBeVisible();
    await expect(page.getByLabel('Confirm password')).toBeVisible();
    await expect(page.getByLabel('Company name')).toBeVisible();
    await expect(page.getByLabel('Legal name')).toBeVisible();
    await expect(page.getByLabel('Base currency')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Create account' })).toBeVisible();
  });

  test('rejects an empty submission with inline validation errors on every required field', async ({ page }) => {
    await page.goto('/register');

    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Full name is required')).toBeVisible();
    await expect(page.getByText('Enter a valid email')).toBeVisible();
    await expect(page.getByText('Password must be at least 10 characters')).toBeVisible();
    await expect(page.getByText('Confirm your password')).toBeVisible();
    await expect(page.getByText('Company name is required')).toBeVisible();
    await expect(page.getByText('Legal name is required')).toBeVisible();
    await expect(page.getByText('Use a 3-letter ISO 4217 code')).toBeVisible();
  });

  test('rejects a malformed email address', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel('Full name').fill('Ada Lovelace');
    await page.getByLabel('Email').fill('not-an-email');
    await page.getByLabel('Password', { exact: true }).fill('longenoughpassword');
    await page.getByLabel('Confirm password').fill('longenoughpassword');
    await page.getByLabel('Company name').fill('Acme Co');
    await page.getByLabel('Legal name').fill('Acme Corporation Ltd');
    await page.getByLabel('Base currency').fill('USD');
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Enter a valid email')).toBeVisible();
  });

  test('rejects a password shorter than 10 characters', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel('Full name').fill('Ada Lovelace');
    await page.getByLabel('Email').fill('ada@example.com');
    await page.getByLabel('Password', { exact: true }).fill('short');
    await page.getByLabel('Confirm password').fill('short');
    await page.getByLabel('Company name').fill('Acme Co');
    await page.getByLabel('Legal name').fill('Acme Corporation Ltd');
    await page.getByLabel('Base currency').fill('USD');
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Password must be at least 10 characters')).toBeVisible();
  });

  test('rejects a confirm password that does not match (cross-field .refine)', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel('Full name').fill('Ada Lovelace');
    await page.getByLabel('Email').fill('ada@example.com');
    await page.getByLabel('Password', { exact: true }).fill('longenoughpassword');
    await page.getByLabel('Confirm password').fill('doesnotmatchpassword');
    await page.getByLabel('Company name').fill('Acme Co');
    await page.getByLabel('Legal name').fill('Acme Corporation Ltd');
    await page.getByLabel('Base currency').fill('USD');
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Passwords do not match')).toBeVisible();
  });

  test('rejects a base currency code that is not exactly 3 letters', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel('Full name').fill('Ada Lovelace');
    await page.getByLabel('Email').fill('ada@example.com');
    await page.getByLabel('Password', { exact: true }).fill('longenoughpassword');
    await page.getByLabel('Confirm password').fill('longenoughpassword');
    await page.getByLabel('Company name').fill('Acme Co');
    await page.getByLabel('Legal name').fill('Acme Corporation Ltd');
    await page.getByLabel('Base currency').fill('US');
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Use a 3-letter ISO 4217 code')).toBeVisible();
  });

  test('has a link back to the login page', async ({ page }) => {
    await page.goto('/register');

    await page.getByRole('link', { name: 'Sign in' }).click();
    await expect(page).toHaveURL(/\/login$/);
  });
});
