import { expect, test } from '@playwright/test';

/**
 * Extends login.spec.ts's single "/core redirects to /login" check to prove
 * RequireAuth (src/app/RequireAuth.tsx) is wired consistently across the
 * whole route table in src/app/AppRoutes.tsx, not just on one lucky route —
 * and that the "from" location it stashes in history state actually differs
 * per route, which is what LoginPage.tsx reads to send a visitor back where
 * they came from post-login (`(location.state as {from?:string})?.from`).
 * No backend is reachable here, so this stays scoped to the client-only
 * routing-guard surface, same rationale as login.spec.ts.
 */
const PROTECTED_ROUTES = ['/core', '/crm', '/manufacturing', '/finance', '/inventory', '/hrms'];

test.describe('RBAC routing guard', () => {
  for (const route of PROTECTED_ROUTES) {
    test(`unauthenticated visit to ${route} redirects to /login`, async ({ page }) => {
      await page.goto(route);
      await expect(page).toHaveURL(/\/login$/);
    });
  }

  test('the redirect stashes the originally-requested path so login can send the visitor back', async ({ page }) => {
    await page.goto('/crm');
    await expect(page).toHaveURL(/\/login$/);

    // RequireAuth does `<Navigate to="/login" replace state={{ from: location.pathname }} />`.
    // BrowserRouter (react-router-dom) mirrors that state onto window.history.state.usr.
    const from = await page.evaluate(() => (window.history.state as { usr?: { from?: string } } | null)?.usr?.from);
    expect(from).toBe('/crm');
  });

  test('a different protected route stashes its own distinct path (guard is per-route, not hardcoded)', async ({ page }) => {
    await page.goto('/manufacturing');
    await expect(page).toHaveURL(/\/login$/);

    const from = await page.evaluate(() => (window.history.state as { usr?: { from?: string } } | null)?.usr?.from);
    expect(from).toBe('/manufacturing');
  });

  test('the customer portal route has its own separate guard (PortalRequireAuth), not RequireAuth', async ({ page }) => {
    await page.goto('/portal');
    await expect(page).toHaveURL(/\/portal\/login$/);
  });
});
