/**
 * Central module registry for the frontend — mirrors FusionOS.Api.Host's
 * ModuleRegistry on the backend (03_SYSTEM_ARCHITECTURE.md). Adding a module's
 * real UI later means adding one entry here, same as the backend pattern.
 */
export interface ModuleNavEntry {
  name: string;
  label: string;
  schema: string;
  phase: string;
  implemented: boolean;
}

export const MODULES: ModuleNavEntry[] = [
  { name: 'core', label: 'Core Platform', schema: 'core', phase: 'Phase 0 — Platform Foundation', implemented: true },
  { name: 'inventory', label: 'Inventory', schema: 'inventory', phase: 'Phase 1 — Trading ERP Core', implemented: true },
  { name: 'warehouse', label: 'Warehouse', schema: 'warehouse', phase: 'Phase 1 — Trading ERP Core', implemented: true },
  { name: 'procurement', label: 'Procurement', schema: 'procurement', phase: 'Phase 1 — Trading ERP Core', implemented: true },
  { name: 'sales', label: 'Sales', schema: 'sales', phase: 'Phase 1 — Trading ERP Core', implemented: true },
  { name: 'finance', label: 'Finance', schema: 'finance', phase: 'Phase 2 — Financial Backbone', implemented: true },
  { name: 'manufacturing', label: 'Manufacturing', schema: 'manufacturing', phase: 'Phase 3 — Manufacturing ERP', implemented: false },
  { name: 'crm', label: 'CRM', schema: 'crm', phase: 'Phase 4 — CRM & HRMS', implemented: false },
  { name: 'hrms', label: 'HRMS', schema: 'hrms', phase: 'Phase 4 — CRM & HRMS', implemented: false },
  { name: 'quality', label: 'Quality', schema: 'quality', phase: 'Phase 5 — Quality & Maintenance', implemented: false },
  { name: 'maintenance', label: 'Maintenance', schema: 'maintenance', phase: 'Phase 5 — Quality & Maintenance', implemented: false },
  { name: 'bi', label: 'Business Intelligence', schema: 'bi', phase: 'Phase 6 — Business Intelligence', implemented: false },
  { name: 'ai', label: 'AI Platform', schema: 'ai', phase: 'Phase 7 — AI Platform', implemented: false },
  { name: 'marketplace', label: 'Marketplace', schema: 'marketplace', phase: 'Phase 8 — Marketplace & Ecosystem', implemented: false },
  { name: 'integration_hub', label: 'Integration Hub', schema: 'integration_hub', phase: 'Phase 9 — Integrations & Mobile', implemented: false },
];
