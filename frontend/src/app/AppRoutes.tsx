import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './layout/AppShell';
import { RequireAuth } from './RequireAuth';
import { LoginPage } from '../modules/core/pages/LoginPage';

const CompaniesPage = lazy(() => import('../modules/core/pages/CompaniesPage').then((m) => ({ default: m.CompaniesPage })));
const ModuleHealthPage = lazy(() => import('../modules/core/pages/ModuleHealthPage').then((m) => ({ default: m.ModuleHealthPage })));
const ProductsPage = lazy(() => import('../modules/inventory/pages/ProductsPage').then((m) => ({ default: m.ProductsPage })));
const WarehousesPage = lazy(() => import('../modules/warehouse/pages/WarehousesPage').then((m) => ({ default: m.WarehousesPage })));
const SuppliersPage = lazy(() => import('../modules/procurement/pages/SuppliersPage').then((m) => ({ default: m.SuppliersPage })));
const CustomersPage = lazy(() => import('../modules/sales/pages/CustomersPage').then((m) => ({ default: m.CustomersPage })));
const AccountsPage = lazy(() => import('../modules/finance/pages/AccountsPage').then((m) => ({ default: m.AccountsPage })));

function RouteFallback() {
  return <p className="p-6 text-sm text-text-muted">Loading…</p>;
}

/**
 * Route table. /login is the one public route — everything else is behind
 * RequireAuth now that the API rejects unauthenticated requests by default
 * (07_SECURITY.md). Core (Companies), Inventory (Products), Warehouse
 * (Warehouses), Procurement (Suppliers), Sales (Customers), and Finance
 * (Accounts) are real screens — see docs/blueprint/05_MODULE_ROADMAP.md Phase
 * 0/1/2. Every other module still resolves to ModuleHealthPage until its
 * Phase begins. Adding a module's next real page means adding one lazy
 * import + one <Route> here, not restructuring this file.
 *
 * Every route element past /login is loaded via React.lazy so each module's
 * page (and its form/table dependencies, including the shared Panel
 * components each page pulls in) ships as its own chunk instead of one
 * monolithic bundle everyone downloads just to see the sign-in screen
 * (02_TECH_STACK.md — Vite's per-module dynamic import splitting, flagged as
 * missing entirely by the audit's Performance pass). LoginPage stays a
 * static import: it is the very first thing an unauthenticated visitor
 * needs, so lazy-loading it would only add a network round trip with no
 * bundle-size benefit. The single <Suspense> boundary around the whole
 * authenticated tree keeps this simple — one shared "Loading…" fallback
 * rather than one per route.
 */
export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route
            index
            element={<Navigate to="/core" replace />}
          />
          <Route
            path="/core"
            element={
              <Suspense fallback={<RouteFallback />}>
                <CompaniesPage />
              </Suspense>
            }
          />
          <Route
            path="/inventory"
            element={
              <Suspense fallback={<RouteFallback />}>
                <ProductsPage />
              </Suspense>
            }
          />
          <Route
            path="/warehouse"
            element={
              <Suspense fallback={<RouteFallback />}>
                <WarehousesPage />
              </Suspense>
            }
          />
          <Route
            path="/procurement"
            element={
              <Suspense fallback={<RouteFallback />}>
                <SuppliersPage />
              </Suspense>
            }
          />
          <Route
            path="/sales"
            element={
              <Suspense fallback={<RouteFallback />}>
                <CustomersPage />
              </Suspense>
            }
          />
          <Route
            path="/finance"
            element={
              <Suspense fallback={<RouteFallback />}>
                <AccountsPage />
              </Suspense>
            }
          />
          <Route
            path="/:moduleName"
            element={
              <Suspense fallback={<RouteFallback />}>
                <ModuleHealthPage />
              </Suspense>
            }
          />
        </Route>
      </Route>
    </Routes>
  );
}
