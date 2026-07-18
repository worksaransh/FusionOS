import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './layout/AppShell';
import { RequireAuth } from './RequireAuth';
import { LoginPage } from '../modules/core/pages/LoginPage';
import { RegisterPage } from '../modules/core/pages/RegisterPage';

const DashboardPage = lazy(() => import('../modules/core/pages/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const CompaniesPage = lazy(() => import('../modules/core/pages/CompaniesPage').then((m) => ({ default: m.CompaniesPage })));
const RolesPage = lazy(() => import('../modules/core/pages/RolesPage').then((m) => ({ default: m.RolesPage })));
const AuditLogPage = lazy(() => import('../modules/core/pages/AuditLogPage').then((m) => ({ default: m.AuditLogPage })));
const SettingsPage = lazy(() => import('../modules/core/pages/SettingsPage').then((m) => ({ default: m.SettingsPage })));
const ApprovalsPage = lazy(() => import('../modules/core/pages/ApprovalsPage').then((m) => ({ default: m.ApprovalsPage })));
const NotificationsPage = lazy(() => import('../modules/core/pages/NotificationsPage').then((m) => ({ default: m.NotificationsPage })));
const ModuleHealthPage = lazy(() => import('../modules/core/pages/ModuleHealthPage').then((m) => ({ default: m.ModuleHealthPage })));
const ProductsPage = lazy(() => import('../modules/inventory/pages/ProductsPage').then((m) => ({ default: m.ProductsPage })));
const WarehousesPage = lazy(() => import('../modules/warehouse/pages/WarehousesPage').then((m) => ({ default: m.WarehousesPage })));
const SuppliersPage = lazy(() => import('../modules/procurement/pages/SuppliersPage').then((m) => ({ default: m.SuppliersPage })));
const CustomersPage = lazy(() => import('../modules/sales/pages/CustomersPage').then((m) => ({ default: m.CustomersPage })));
const AccountsPage = lazy(() => import('../modules/finance/pages/AccountsPage').then((m) => ({ default: m.AccountsPage })));
const BillsOfMaterialsPage = lazy(() => import('../modules/manufacturing/pages/BillsOfMaterialsPage').then((m) => ({ default: m.BillsOfMaterialsPage })));
const LeadsPage = lazy(() => import('../modules/crm/pages/LeadsPage').then((m) => ({ default: m.LeadsPage })));
const InspectionsPage = lazy(() => import('../modules/quality/pages/InspectionsPage').then((m) => ({ default: m.InspectionsPage })));
const AssetsPage = lazy(() => import('../modules/maintenance/pages/AssetsPage').then((m) => ({ default: m.AssetsPage })));
const EmployeesPage = lazy(() => import('../modules/hrms/pages/EmployeesPage').then((m) => ({ default: m.EmployeesPage })));
const KpiDefinitionsPage = lazy(() => import('../modules/bi/pages/KpiDefinitionsPage').then((m) => ({ default: m.KpiDefinitionsPage })));
const RecommendationsPage = lazy(() => import('../modules/ai/pages/RecommendationsPage').then((m) => ({ default: m.RecommendationsPage })));
const PluginListingsPage = lazy(() => import('../modules/marketplace/pages/PluginListingsPage').then((m) => ({ default: m.PluginListingsPage })));
const IntegrationConnectorsPage = lazy(() => import('../modules/integration_hub/pages/IntegrationConnectorsPage').then((m) => ({ default: m.IntegrationConnectorsPage })));

function RouteFallback() {
  return <p className="p-6 text-sm text-text-muted">Loading…</p>;
}

/**
 * Route table. /login and /register are the two public routes — everything
 * else is behind RequireAuth now that the API rejects unauthenticated
 * requests by default (07_SECURITY.md). Core (Companies, Roles, Audit Log),
 * Inventory (Products), Warehouse (Warehouses), Procurement (Suppliers),
 * Sales (Customers), and Finance (Accounts) are real screens — see
 * docs/blueprint/05_MODULE_ROADMAP.md Phase 0/1/2 and
 * docs/REMEDIATION_ROADMAP.md Phase H. Every other module still resolves to
 * ModuleHealthPage until its Phase begins. Adding a module's next real page
 * means adding one lazy import + one <Route> here, not restructuring this
 * file.
 *
 * Every route element past /login//register is loaded via React.lazy so each
 * module's page (and its form/table dependencies, including the shared Panel
 * components each page pulls in) ships as its own chunk instead of one
 * monolithic bundle everyone downloads just to see the sign-in screen
 * (02_TECH_STACK.md — Vite's per-module dynamic import splitting, flagged as
 * missing entirely by the audit's Performance pass). LoginPage/RegisterPage
 * stay static imports: they are the first things an unauthenticated visitor
 * may need, so lazy-loading them would only add a network round trip with no
 * bundle-size benefit. The single <Suspense> boundary around the whole
 * authenticated tree keeps this simple — one shared "Loading…" fallback
 * rather than one per route.
 *
 * / redirects to /dashboard (Phase M6, 2026-07-15) — the cross-module KPI
 * landing page — rather than /core, now that there's a real landing page to
 * send people to instead of the Companies list.
 */
export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route
            index
            element={<Navigate to="/dashboard" replace />}
          />
          <Route
            path="/dashboard"
            element={
              <Suspense fallback={<RouteFallback />}>
                <DashboardPage />
              </Suspense>
            }
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
            path="/core/roles"
            element={
              <Suspense fallback={<RouteFallback />}>
                <RolesPage />
              </Suspense>
            }
          />
          <Route
            path="/core/audit-log"
            element={
              <Suspense fallback={<RouteFallback />}>
                <AuditLogPage />
              </Suspense>
            }
          />
          <Route
            path="/core/settings"
            element={
              <Suspense fallback={<RouteFallback />}>
                <SettingsPage />
              </Suspense>
            }
          />
          <Route
            path="/core/approvals"
            element={
              <Suspense fallback={<RouteFallback />}>
                <ApprovalsPage />
              </Suspense>
            }
          />
          <Route
            path="/core/notifications"
            element={
              <Suspense fallback={<RouteFallback />}>
                <NotificationsPage />
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
            path="/manufacturing"
            element={
              <Suspense fallback={<RouteFallback />}>
                <BillsOfMaterialsPage />
              </Suspense>
            }
          />
          <Route
            path="/crm"
            element={
              <Suspense fallback={<RouteFallback />}>
                <LeadsPage />
              </Suspense>
            }
          />
          <Route
            path="/quality"
            element={
              <Suspense fallback={<RouteFallback />}>
                <InspectionsPage />
              </Suspense>
            }
          />
          <Route
            path="/maintenance"
            element={
              <Suspense fallback={<RouteFallback />}>
                <AssetsPage />
              </Suspense>
            }
          />
          <Route
            path="/hrms"
            element={
              <Suspense fallback={<RouteFallback />}>
                <EmployeesPage />
              </Suspense>
            }
          />
          <Route
            path="/bi"
            element={
              <Suspense fallback={<RouteFallback />}>
                <KpiDefinitionsPage />
              </Suspense>
            }
          />
          <Route
            path="/ai"
            element={
              <Suspense fallback={<RouteFallback />}>
                <RecommendationsPage />
              </Suspense>
            }
          />
          <Route
            path="/marketplace"
            element={
              <Suspense fallback={<RouteFallback />}>
                <PluginListingsPage />
              </Suspense>
            }
          />
          <Route
            path="/integration_hub"
            element={
              <Suspense fallback={<RouteFallback />}>
                <IntegrationConnectorsPage />
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
