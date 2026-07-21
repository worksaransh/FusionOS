import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './layout/AppShell';
import { RequireAuth } from './RequireAuth';
import { RequirePermission } from './RequirePermission';
import { LoginPage } from '../modules/core/pages/LoginPage';
import { RegisterPage } from '../modules/core/pages/RegisterPage';

const DashboardPage = lazy(() => import('../modules/core/pages/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const CompaniesPage = lazy(() => import('../modules/core/pages/CompaniesPage').then((m) => ({ default: m.CompaniesPage })));
const RolesPage = lazy(() => import('../modules/core/pages/RolesPage').then((m) => ({ default: m.RolesPage })));
const AuditLogPage = lazy(() => import('../modules/core/pages/AuditLogPage').then((m) => ({ default: m.AuditLogPage })));
const SettingsPage = lazy(() => import('../modules/core/pages/SettingsPage').then((m) => ({ default: m.SettingsPage })));
const ApprovalsPage = lazy(() => import('../modules/core/pages/ApprovalsPage').then((m) => ({ default: m.ApprovalsPage })));
const FeatureFlagsPage = lazy(() => import('../modules/core/pages/FeatureFlagsPage').then((m) => ({ default: m.FeatureFlagsPage })));
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
 *
 * Every real module page (everything except /dashboard, /core/approvals and
 * /core/notifications, which every authenticated user can reach regardless
 * of role — see AppShell's docstring) is wrapped in RequirePermission with
 * that module's primary "*.read" permission code from PermissionCatalog.cs.
 * This mirrors the sidebar's own per-module filtering in AppShell.tsx so a
 * user who somehow navigates straight to a URL they have no nav link for
 * sees a "Not authorized" page instead of a broken/empty one — additive
 * UI-layer gating on top of the backend's real IRequirePermission checks,
 * not a replacement for them.
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
              <RequirePermission permission="core.company.read">
                <Suspense fallback={<RouteFallback />}>
                  <CompaniesPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/core/roles"
            element={
              <RequirePermission permission="core.role.manage">
                <Suspense fallback={<RouteFallback />}>
                  <RolesPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/core/audit-log"
            element={
              <RequirePermission permission="core.audit.read">
                <Suspense fallback={<RouteFallback />}>
                  <AuditLogPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/core/settings"
            element={
              <RequirePermission permission="core.settings.read">
                <Suspense fallback={<RouteFallback />}>
                  <SettingsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/core/feature-flags"
            element={
              <RequirePermission permission="core.feature-flag.read">
                <Suspense fallback={<RouteFallback />}>
                  <FeatureFlagsPage />
                </Suspense>
              </RequirePermission>
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
              <RequirePermission permission="inventory.product.read">
                <Suspense fallback={<RouteFallback />}>
                  <ProductsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/warehouse"
            element={
              <RequirePermission permission="warehouse.warehouse.read">
                <Suspense fallback={<RouteFallback />}>
                  <WarehousesPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/procurement"
            element={
              <RequirePermission permission="procurement.supplier.read">
                <Suspense fallback={<RouteFallback />}>
                  <SuppliersPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/sales"
            element={
              <RequirePermission permission="sales.customer.read">
                <Suspense fallback={<RouteFallback />}>
                  <CustomersPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/finance"
            element={
              <RequirePermission permission="finance.account.read">
                <Suspense fallback={<RouteFallback />}>
                  <AccountsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/manufacturing"
            element={
              <RequirePermission permission="manufacturing.bill-of-materials.read">
                <Suspense fallback={<RouteFallback />}>
                  <BillsOfMaterialsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/crm"
            element={
              <RequirePermission permission="crm.lead.read">
                <Suspense fallback={<RouteFallback />}>
                  <LeadsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/quality"
            element={
              <RequirePermission permission="quality.inspection.read">
                <Suspense fallback={<RouteFallback />}>
                  <InspectionsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/maintenance"
            element={
              <RequirePermission permission="maintenance.asset.read">
                <Suspense fallback={<RouteFallback />}>
                  <AssetsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/hrms"
            element={
              <RequirePermission permission="hrms.employee.read">
                <Suspense fallback={<RouteFallback />}>
                  <EmployeesPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/bi"
            element={
              <RequirePermission permission="bi.kpi-definition.read">
                <Suspense fallback={<RouteFallback />}>
                  <KpiDefinitionsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/ai"
            element={
              <RequirePermission permission="ai.recommendation.read">
                <Suspense fallback={<RouteFallback />}>
                  <RecommendationsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/marketplace"
            element={
              <RequirePermission permission="marketplace.plugin-listing.read">
                <Suspense fallback={<RouteFallback />}>
                  <PluginListingsPage />
                </Suspense>
              </RequirePermission>
            }
          />
          <Route
            path="/integration_hub"
            element={
              <RequirePermission permission="integration_hub.connector.read">
                <Suspense fallback={<RouteFallback />}>
                  <IntegrationConnectorsPage />
                </Suspense>
              </RequirePermission>
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
