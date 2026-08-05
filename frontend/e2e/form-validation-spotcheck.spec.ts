import { expect, test, type Page } from '@playwright/test';

/**
 * Spot-checks the same class of bug the login page had (native HTML5
 * constraint validation silently swallowing submission before Zod ever ran
 * — fixed repo-wide by adding `noValidate` to every <form>) on a handful of
 * *authenticated* forms across different modules: CRM (LeadsPage), Manufacturing
 * (BillsOfMaterialsPage) and Finance (ExchangeRatesPanel, nested in
 * AccountsPage). Each is picked because it has a field type the browser can
 * natively constrain (an `email`-shaped value, a numeric/positive-only value,
 * or an `<input type="date">`), so it's exactly the shape of field that broke
 * before the bulk fix.
 *
 * These pages sit behind RequireAuth + RequirePermission, and no backend is
 * reachable in this environment (no .NET SDK — see e2e/login.spec.ts), so a
 * real login can't happen. Instead we seed the zustand-persisted auth/company
 * stores directly into localStorage before navigating — this is the same
 * client-only surface login.spec.ts/register.spec.ts stay on, just extended
 * far enough to reach a rendered form past the route guards without faking
 * a network response. Anything the form does past *client-side* validation
 * (the create mutation itself) is out of scope here, same as everywhere else
 * in this suite.
 */
const FAKE_COMPANY_ID = '11111111-1111-1111-1111-111111111111';

async function signInAsFakeUser(page: Page, permissions: string[]) {
  await page.addInitScript(
    ({ companyId, perms }) => {
      window.localStorage.setItem(
        'fusionos.auth-session',
        JSON.stringify({
          state: {
            session: {
              userId: 'e2e-user',
              email: 'e2e@example.com',
              fullName: 'E2E Test User',
              companyId,
              branchId: null,
              permissions: perms,
              accessToken: 'fake-access-token',
              accessTokenExpiresAt: new Date(Date.now() + 3600_000).toISOString(),
              refreshToken: 'fake-refresh-token',
              refreshTokenExpiresAt: new Date(Date.now() + 3600_000).toISOString(),
            },
          },
          version: 0,
        }),
      );
      window.localStorage.setItem(
        'fusionos.active-company',
        JSON.stringify({ state: { companyId }, version: 0 }),
      );
    },
    { companyId: FAKE_COMPANY_ID, perms: permissions },
  );
}

test.describe('CRM — Leads create form (LeadsPage.tsx)', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsFakeUser(page, ['crm.lead.read']);
  });

  test('rejects an empty submission and a malformed contact email without a native-validation short-circuit', async ({ page }) => {
    await page.goto('/crm');
    await expect(page).toHaveURL(/\/crm$/);

    // LeadsPage stacks several sibling panels (Opportunities, Contacts, Accounts...)
    // that each have their own "Name"/"Email" labeled fields, so scope to the
    // specific create-lead form (identified by its submit button) rather than
    // a bare getByLabel, which is ambiguous across the whole page.
    const form = page.locator('form', { has: page.getByRole('button', { name: 'Create lead' }) });

    await form.getByLabel('Name').fill('');
    await form.getByLabel('Contact email (optional)').fill('not-an-email');
    await form.getByRole('button', { name: 'Create lead' }).click();

    await expect(form.getByText('Name is required')).toBeVisible();
    await expect(form.getByText('Must be a valid email')).toBeVisible();
  });

  test('accepts a blank contact email (field is optional)', async ({ page }) => {
    await page.goto('/crm');

    const form = page.locator('form', { has: page.getByRole('button', { name: 'Create lead' }) });

    await form.getByLabel('Name').fill('Acme Prospect');
    await form.getByRole('button', { name: 'Create lead' }).click();

    await expect(form.getByText('Must be a valid email')).not.toBeVisible();
  });
});

test.describe('Manufacturing — Bill of Materials create form (BillsOfMaterialsPage.tsx)', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsFakeUser(page, ['manufacturing.bill-of-materials.read']);
  });

  test('rejects an empty submission and a non-positive component quantity', async ({ page }) => {
    await page.goto('/manufacturing');
    await expect(page).toHaveURL(/\/manufacturing$/);

    // BillsOfMaterialsPage also renders WorkOrdersPanel/MrpPanel sibling panels,
    // each with their own "Quantity"-labeled field, so scope to this specific
    // create-BOM form via its submit button rather than a bare getByLabel.
    const form = page.locator('form', { has: page.getByRole('button', { name: 'Create bill of materials' }) });

    await form.getByLabel('Quantity').fill('-5');
    await form.getByRole('button', { name: 'Create bill of materials' }).click();

    await expect(form.getByText('Code is required')).toBeVisible();
    await expect(form.getByText('Name is required')).toBeVisible();
    await expect(form.getByText('Quantity must be greater than zero')).toBeVisible();
  });
});

test.describe('Finance — Exchange Rates create form (ExchangeRatesPanel.tsx)', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsFakeUser(page, ['finance.account.read']);
  });

  test('rejects an empty submission, a non-3-letter currency code, and a non-positive rate', async ({ page }) => {
    await page.goto('/finance');
    await expect(page).toHaveURL(/\/finance$/);

    // ExchangeRatesPanel only renders under the "Banking & Treasury" sub-tab
    // (AccountsPage.tsx defaults activeFinanceTab to 'gl') — switch tabs first.
    await page.getByRole('button', { name: 'Banking & Treasury' }).click();

    await page.getByLabel('From currency').fill('US');
    await page.getByLabel('To currency').fill('EUR');
    await page.getByLabel('Rate (1 From = Rate To)').fill('-1');
    await page.getByRole('button', { name: 'Create exchange rate' }).click();

    await expect(page.getByText('Must be a 3-letter ISO 4217 code, e.g. USD')).toBeVisible();
    await expect(page.getByText('Rate must be greater than zero')).toBeVisible();
    await expect(page.getByText('Effective date is required')).toBeVisible();
  });

  test('the effective date field is a native date input but still Zod-validated through noValidate', async ({ page }) => {
    await page.goto('/finance');

    // ExchangeRatesPanel only renders under the "Banking & Treasury" sub-tab.
    await page.getByRole('button', { name: 'Banking & Treasury' }).click();

    const dateInput = page.getByLabel('Effective date');
    await expect(dateInput).toHaveAttribute('type', 'date');

    // Submitting with the date left blank must surface the Zod message, not
    // silently no-op via the browser's native required-field handling for
    // <input type="date"> (there's no `required` attribute here, but this
    // guards against a future regression that adds one without noValidate).
    await page.getByLabel('From currency').fill('USD');
    await page.getByLabel('To currency').fill('EUR');
    await page.getByLabel('Rate (1 From = Rate To)').fill('0.92');
    await page.getByRole('button', { name: 'Create exchange rate' }).click();

    await expect(page.getByText('Effective date is required')).toBeVisible();
  });
});
