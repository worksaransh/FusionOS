import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from './client';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import type { PagedResult } from './types';
import type { EntityOption } from '../ui/EntityCombobox';

/**
 * Backs every EntityCombobox in the app (shared/ui/EntityCombobox.tsx) — one
 * hook per pickable entity type. Product, Supplier, Customer, Warehouse, and
 * Account now search server-side (their list endpoints accept a `search`
 * query param — 08_API_STANDARDS.md), debounced here so keystrokes don't
 * each fire a request; the returned `onSearchChange` is passed straight to
 * EntityCombobox. Zone, Sales Order, and Purchase Order list endpoints don't
 * have search support yet, so those three hooks still fetch one page and
 * fall back to EntityCombobox's client-side filtering (fine at their current
 * scale — a warehouse's zones, one company's orders — unlike the
 * company-wide Product/Customer/Supplier lists this was actually fixing).
 */
const OPTIONS_PAGE_SIZE = 20;
const SEARCH_DEBOUNCE_MS = 250;

function useEntityOptions<T>(
  queryKeyPrefix: string,
  basePath: string,
  companyId: string | undefined,
  mapToOption: (item: T) => EntityOption,
): { options: EntityOption[]; isLoading: boolean; onSearchChange: (search: string) => void } {
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);
  const enabled = Boolean(companyId);

  const query = useQuery({
    queryKey: [queryKeyPrefix, companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId: companyId!, page: '1', pageSize: String(OPTIONS_PAGE_SIZE) });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<T>>(`${basePath}?${params.toString()}`);
    },
    enabled,
  });

  return {
    options: query.data?.data.map(mapToOption) ?? [],
    isLoading: query.isLoading,
    onSearchChange: setSearch,
  };
}

/** For entities whose list endpoint has no `search` param yet — one generous page, client-side filtered by EntityCombobox itself. */
function useEntityOptionsWithoutSearch<T>(
  queryKey: readonly unknown[],
  path: string,
  enabled: boolean,
  mapToOption: (item: T) => EntityOption,
): { options: EntityOption[]; isLoading: boolean } {
  const query = useQuery({
    queryKey,
    queryFn: () => apiClient.get<PagedResult<T>>(path),
    enabled,
  });

  return {
    options: query.data?.data.map(mapToOption) ?? [],
    isLoading: query.isLoading,
  };
}

interface ProductLite {
  id: string;
  sku: string;
  name: string;
}

export function useProductOptions(companyId: string | undefined) {
  return useEntityOptions<ProductLite>(
    'product-options',
    '/inventory/products',
    companyId,
    (p) => ({ id: p.id, label: `${p.sku} — ${p.name}` }),
  );
}

interface SupplierLite {
  id: string;
  code: string;
  name: string;
}

export function useSupplierOptions(companyId: string | undefined) {
  return useEntityOptions<SupplierLite>(
    'supplier-options',
    '/procurement/suppliers',
    companyId,
    (s) => ({ id: s.id, label: `${s.code} — ${s.name}` }),
  );
}

interface CustomerLite {
  id: string;
  code: string;
  name: string;
}

export function useCustomerOptions(companyId: string | undefined) {
  return useEntityOptions<CustomerLite>(
    'customer-options',
    '/sales/customers',
    companyId,
    (c) => ({ id: c.id, label: `${c.code} — ${c.name}` }),
  );
}

interface WarehouseLite {
  id: string;
  code: string;
  name: string;
}

export function useWarehouseOptions(companyId: string | undefined) {
  return useEntityOptions<WarehouseLite>(
    'warehouse-options',
    '/warehouse/warehouses',
    companyId,
    (w) => ({ id: w.id, label: `${w.code} — ${w.name}` }),
  );
}

interface ZoneLite {
  id: string;
  code: string;
  name: string;
}

const ZONE_OPTIONS_PAGE_SIZE = 200;

export function useZoneOptions(companyId: string | undefined, warehouseId: string | undefined) {
  return useEntityOptionsWithoutSearch<ZoneLite>(
    ['zone-options', companyId, warehouseId],
    `/warehouse/warehouses/${warehouseId}/zones?companyId=${companyId}&page=1&pageSize=${ZONE_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId && warehouseId),
    (z) => ({ id: z.id, label: `${z.code} — ${z.name}` }),
  );
}

interface BinLite {
  id: string;
  code: string;
  name: string;
}

const BIN_OPTIONS_PAGE_SIZE = 200;

/** Bins nest under Zone one level deeper than Zone nests under Warehouse (Phase M9, 2026-07-15) — same no-search-yet, one-page pattern as useZoneOptions. */
export function useBinOptions(companyId: string | undefined, warehouseId: string | undefined, zoneId: string | undefined) {
  return useEntityOptionsWithoutSearch<BinLite>(
    ['bin-options', companyId, warehouseId, zoneId],
    `/warehouse/warehouses/${warehouseId}/zones/${zoneId}/bins?companyId=${companyId}&page=1&pageSize=${BIN_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId && warehouseId && zoneId),
    (b) => ({ id: b.id, label: `${b.code} — ${b.name}` }),
  );
}

interface AccountLite {
  id: string;
  code: string;
  name: string;
}

export function useAccountOptions(companyId: string | undefined) {
  return useEntityOptions<AccountLite>(
    'account-options',
    '/finance/accounts',
    companyId,
    (a) => ({ id: a.id, label: `${a.code} — ${a.name}` }),
  );
}

interface LeadLite {
  id: string;
  name: string;
  status: string;
}

/** Backs OpportunitiesPanel's lead picker (CRM frontend, 2026-07-18) — leads' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useLeadOptions(companyId: string | undefined) {
  return useEntityOptions<LeadLite>(
    'lead-options',
    '/crm/leads',
    companyId,
    (l) => ({ id: l.id, label: `${l.name} (${l.status})` }),
  );
}

interface OpportunityLite {
  id: string;
  name: string;
  stage: string;
}

const OPPORTUNITY_OPTIONS_PAGE_SIZE = 200;

/** Backs ActivitiesPanel's opportunity picker (CRM depth pass, 2026-07-20) — ListOpportunitiesQuery has no `search` param yet, same one-page/client-filtered pattern as useSalesOrderOptions. */
export function useOpportunityOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<OpportunityLite>(
    ['opportunity-options', companyId],
    `/crm/opportunities?companyId=${companyId}&page=1&pageSize=${OPPORTUNITY_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (o) => ({ id: o.id, label: `${o.name} (${o.stage})` }),
  );
}

interface CrmAccountLite {
  id: string;
  name: string;
  industry: string | null;
}

/**
 * Backs ContactsPanel's/ActivitiesPanel's/Lead-Opportunity assign-account pickers (CRM
 * depth pass, 2026-07-20). Named "Crm" to disambiguate from Finance's chart-of-accounts
 * useAccountOptions above — same server-side search pattern, different backing endpoint
 * (/crm/accounts, the org/company behind a lead/opportunity/contact — see Account.cs).
 */
export function useCrmAccountOptions(companyId: string | undefined) {
  return useEntityOptions<CrmAccountLite>(
    'crm-account-options',
    '/crm/accounts',
    companyId,
    (a) => ({ id: a.id, label: a.industry ? `${a.name} (${a.industry})` : a.name }),
  );
}

interface ContactLite {
  id: string;
  name: string;
  email: string | null;
}

/** Backs ActivitiesPanel's contact picker (CRM depth pass, 2026-07-20) — contacts' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useContactOptions(companyId: string | undefined) {
  return useEntityOptions<ContactLite>(
    'contact-options',
    '/crm/contacts',
    companyId,
    (c) => ({ id: c.id, label: c.email ? `${c.name} (${c.email})` : c.name }),
  );
}

interface BillOfMaterialsLite {
  id: string;
  code: string;
  name: string;
}

/** Backs WorkOrdersPanel's bill-of-materials picker (Manufacturing frontend, 2026-07-18) — bills-of-materials' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useBillOfMaterialsOptions(companyId: string | undefined) {
  return useEntityOptions<BillOfMaterialsLite>(
    'bill-of-materials-options',
    '/manufacturing/bills-of-materials',
    companyId,
    (b) => ({ id: b.id, label: `${b.code} — ${b.name}` }),
  );
}

interface IntegrationConnectorLite {
  id: string;
  code: string;
  name: string;
}

/** Backs ConnectorConnectionsPanel's connector picker (Integration Hub frontend, 2026-07-18) — connectors' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useIntegrationConnectorOptions(companyId: string | undefined) {
  return useEntityOptions<IntegrationConnectorLite>(
    'integration-connector-options',
    '/integration-hub/connectors',
    companyId,
    (c) => ({ id: c.id, label: `${c.code} — ${c.name}` }),
  );
}

interface PluginListingLite {
  id: string;
  code: string;
  name: string;
}

/** Backs PluginInstallationsPanel's listing picker (Marketplace frontend, 2026-07-18) — plugin-listings' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function usePluginListingOptions(companyId: string | undefined) {
  return useEntityOptions<PluginListingLite>(
    'plugin-listing-options',
    '/marketplace/plugin-listings',
    companyId,
    (p) => ({ id: p.id, label: `${p.code} — ${p.name}` }),
  );
}

interface KpiDefinitionLite {
  id: string;
  code: string;
  name: string;
}

/** Backs KpiSnapshotsPanel's KPI picker (Business Intelligence frontend, 2026-07-18) — kpi-definitions' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useKpiDefinitionOptions(companyId: string | undefined) {
  return useEntityOptions<KpiDefinitionLite>(
    'kpi-definition-options',
    '/bi/kpi-definitions',
    companyId,
    (k) => ({ id: k.id, label: `${k.code} — ${k.name}` }),
  );
}

interface EmployeeLite {
  id: string;
  code: string;
  fullName: string;
}

/** Backs LeaveRequestsPanel's employee picker (HRMS frontend, 2026-07-18) — employees' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useEmployeeOptions(companyId: string | undefined) {
  return useEntityOptions<EmployeeLite>(
    'employee-options',
    '/hrms/employees',
    companyId,
    (e) => ({ id: e.id, label: `${e.code} — ${e.fullName}` }),
  );
}

interface LeaveRequestLite {
  id: string;
  employeeId: string;
  type: string;
  startDate: string;
  endDate: string;
  status: string;
}

const LEAVE_REQUEST_OPTIONS_PAGE_SIZE = 200;

/**
 * Backs AttendancePanel's optional "linked leave request" picker (HRMS
 * frontend, 2026-07-20) — leave-requests' list endpoint has no `search` param
 * yet, same one-page/client-filtered pattern as useZoneOptions, scoped to the
 * employee already selected in the same form (mirrors useZoneOptions being
 * scoped to a warehouseId).
 */
export function useLeaveRequestOptions(companyId: string | undefined, employeeId: string | undefined) {
  return useEntityOptionsWithoutSearch<LeaveRequestLite>(
    ['leave-request-options', companyId, employeeId],
    `/hrms/leave-requests?companyId=${companyId}&employeeId=${employeeId}&page=1&pageSize=${LEAVE_REQUEST_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId && employeeId),
    (l) => ({
      id: l.id,
      label: `${l.type} · ${new Date(l.startDate).toLocaleDateString()}–${new Date(l.endDate).toLocaleDateString()} · ${l.status}`,
    }),
  );
}

interface AssetLite {
  id: string;
  code: string;
  name: string;
}

/** Backs MaintenanceRequestsPanel's asset picker (Maintenance frontend, 2026-07-18) — assets' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useAssetOptions(companyId: string | undefined) {
  return useEntityOptions<AssetLite>(
    'asset-options',
    '/maintenance/assets',
    companyId,
    (a) => ({ id: a.id, label: `${a.code} — ${a.name}` }),
  );
}

interface CostCenterLite {
  id: string;
  code: string;
  name: string;
}

/** Backs BudgetsPanel's cost-center picker (M8f, 2026-07-17) — cost-centers' list endpoint supports `search`, same server-side pattern as useAccountOptions. First hook for CostCenter — no earlier M8a frontend work added one. */
export function useCostCenterOptions(companyId: string | undefined) {
  return useEntityOptions<CostCenterLite>(
    'cost-center-options',
    '/finance/cost-centers',
    companyId,
    (c) => ({ id: c.id, label: `${c.code} — ${c.name}` }),
  );
}

interface TaxJurisdictionLite {
  id: string;
  code: string;
  name: string;
}

/** Backs TaxRatesPanel's jurisdiction picker (M8b, 2026-07-17) — tax-jurisdictions' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useTaxJurisdictionOptions(companyId: string | undefined) {
  return useEntityOptions<TaxJurisdictionLite>(
    'tax-jurisdiction-options',
    '/finance/tax-jurisdictions',
    companyId,
    (j) => ({ id: j.id, label: `${j.code} — ${j.name}` }),
  );
}

interface TaxRateLite {
  id: string;
  code: string;
  name: string;
  percentage: number;
}

const TAX_RATE_OPTIONS_PAGE_SIZE = 100;

/**
 * Backs InvoicesPanel's/PurchaseOrdersPanel's per-line tax-rate picker (Phase 2
 * closeout, 2026-07-18 — wiring the previously-unused CalculateLineTaxQuery
 * into transaction lines). Tax rates' list endpoint requires a
 * `taxJurisdictionId` (a rate nests under one jurisdiction, not a company-wide
 * list) and has no `search` param, so this only fetches once a jurisdiction is
 * picked, one page, client-filtered — same `useEntityOptionsWithoutSearch`
 * shape as usePurchaseOrderOptions.
 */
export function useTaxRateOptions(companyId: string | undefined, taxJurisdictionId: string | undefined) {
  return useEntityOptionsWithoutSearch<TaxRateLite>(
    ['tax-rate-options', companyId, taxJurisdictionId],
    `/finance/tax-rates?companyId=${companyId}&taxJurisdictionId=${taxJurisdictionId}&page=1&pageSize=${TAX_RATE_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId && taxJurisdictionId),
    (r) => ({ id: r.id, label: `${r.code} — ${r.name} (${r.percentage}%)` }),
  );
}

interface BankAccountLite {
  id: string;
  code: string;
  name: string;
}

/** Backs BankStatementLinesPanel's bank-account picker (M8d, 2026-07-17) — bank-accounts' list endpoint supports `search`, same server-side pattern as useAccountOptions/useTaxJurisdictionOptions. */
export function useBankAccountOptions(companyId: string | undefined) {
  return useEntityOptions<BankAccountLite>(
    'bank-account-options',
    '/finance/bank-accounts',
    companyId,
    (b) => ({ id: b.id, label: `${b.code} — ${b.name}` }),
  );
}

interface OrderLite {
  id: string;
  status: string;
  orderDate: string;
  totalAmount: number;
}

const ORDER_OPTIONS_PAGE_SIZE = 200;

export function useSalesOrderOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<OrderLite>(
    ['sales-order-options', companyId],
    `/sales/sales-orders?companyId=${companyId}&page=1&pageSize=${ORDER_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (o) => ({
      id: o.id,
      label: `${new Date(o.orderDate).toLocaleDateString()} · ${o.status} · ${o.totalAmount.toLocaleString()}`,
    }),
  );
}

export function usePurchaseOrderOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<OrderLite>(
    ['purchase-order-options', companyId],
    `/procurement/purchase-orders?companyId=${companyId}&page=1&pageSize=${ORDER_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (o) => ({
      id: o.id,
      label: `${new Date(o.orderDate).toLocaleDateString()} · ${o.status} · ${o.totalAmount.toLocaleString()}`,
    }),
  );
}

interface InvoiceLite {
  id: string;
  invoiceDate: string;
  status: string;
  totalAmount: number;
}

const INVOICE_OPTIONS_PAGE_SIZE = 200;

/** Sales' invoice list endpoint has no `search` param yet, same as Sales Order/Purchase Order above — one page, client-side filtered by EntityCombobox. */
export function useInvoiceOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<InvoiceLite>(
    ['invoice-options', companyId],
    `/sales/invoices?companyId=${companyId}&page=1&pageSize=${INVOICE_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (i) => ({
      id: i.id,
      label: `${new Date(i.invoiceDate).toLocaleDateString()} · ${i.status} · ${i.totalAmount.toLocaleString()}`,
    }),
  );
}

interface JournalEntryLite {
  id: string;
  reference: string | null;
  entryDate: string;
  totalDebit: number;
}

const JOURNAL_ENTRY_OPTIONS_PAGE_SIZE = 200;

/** Backs BankStatementLinesPanel's optional "matched journal entry" picker (M8d, 2026-07-17) — journal-entries' list endpoint has no `search` param yet, same one-page/client-filtered pattern as useSalesOrderOptions/usePurchaseOrderOptions. */
export function useJournalEntryOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<JournalEntryLite>(
    ['journal-entry-options', companyId],
    `/finance/journal-entries?companyId=${companyId}&page=1&pageSize=${JOURNAL_ENTRY_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (j) => ({
      id: j.id,
      label: j.reference
        ? `${j.reference} · ${new Date(j.entryDate).toLocaleDateString()} · ${j.totalDebit.toLocaleString()}`
        : `${new Date(j.entryDate).toLocaleDateString()} · ${j.totalDebit.toLocaleString()}`,
    }),
  );
}

interface CompanyUserLite {
  userId: string;
  email: string;
  fullName: string;
}

/**
 * Backs the approver picker on ApprovalsPage (Phase M7, 2026-07-15). Users'
 * list endpoint (`/core/users`) returns a plain array, not a PagedResult
 * (ListCompanyUsersQuery has no pagination today — a company's user roster
 * is small), so this can't reuse useEntityOptions' PagedResult-shaped
 * machinery; it's a small bespoke hook instead.
 */
export function useUserOptions(companyId: string | undefined) {
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);
  const enabled = Boolean(companyId);

  const query = useQuery({
    queryKey: ['user-options', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId: companyId! });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<CompanyUserLite[]>(`/core/users?${params.toString()}`);
    },
    enabled,
  });

  return {
    options: (query.data ?? []).map((u) => ({ id: u.userId, label: `${u.fullName} (${u.email})` })),
    isLoading: query.isLoading,
    onSearchChange: setSearch,
  };
}

interface InspectionLite {
  id: string;
  type: string;
  status: string;
}

const INSPECTION_OPTIONS_PAGE_SIZE = 200;

/** Backs NonConformanceReportsPanel's optional Inspection-link picker (Quality NCR/CAPA slice) — inspections' list endpoint has no `search` param yet, same one-page/client-filtered pattern as useSalesOrderOptions/usePurchaseOrderOptions. */
export function useInspectionOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<InspectionLite>(
    ['inspection-options', companyId],
    `/quality/inspections?companyId=${companyId}&page=1&pageSize=${INSPECTION_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (i) => ({ id: i.id, label: `${i.type} · ${i.status}` }),
  );
}

interface BranchLite {
  id: string;
  code: string;
  name: string;
}

/** Backs DepartmentsPanel's branch picker (Core Organizations frontend, 2026-07-21) — branches' list endpoint supports `search`, same server-side pattern as useAccountOptions. */
export function useBranchOptions(companyId: string | undefined) {
  return useEntityOptions<BranchLite>(
    'branch-options',
    '/core/branches',
    companyId,
    (b) => ({ id: b.id, label: `${b.code} — ${b.name}` }),
  );
}

interface NonConformanceReportLite {
  id: string;
  description: string;
  severity: string;
  status: string;
}

const NON_CONFORMANCE_REPORT_OPTIONS_PAGE_SIZE = 200;

/** Backs CorrectiveActionsPanel's NCR-link picker (Quality NCR/CAPA slice) — non-conformance-reports' list endpoint has no `search` param yet, same one-page/client-filtered pattern as useInspectionOptions. */
export function useNonConformanceReportOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<NonConformanceReportLite>(
    ['non-conformance-report-options', companyId],
    `/quality/non-conformance-reports?companyId=${companyId}&page=1&pageSize=${NON_CONFORMANCE_REPORT_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (r) => ({ id: r.id, label: `${r.severity} — ${r.description}` }),
  );
}
