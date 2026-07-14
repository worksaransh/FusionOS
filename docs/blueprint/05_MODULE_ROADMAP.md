# 05_MODULE_ROADMAP.md — FusionOS Module List & Phased Roadmap

Every module below follows the layering and event rules in `03_SYSTEM_ARCHITECTURE.md` and the database rules in `04_DATABASE_GUIDELINES.md`. No module ships without database, API, frontend, tests, validation, docs, error handling, and security/performance review (`01_PROJECT_RULES.md` — Quality Bar).

## 1. Full Module Catalog

### Core Platform
Authentication · RBAC · Organizations · Companies · Branches · Departments · Employees (identity records) · Notifications · Workflow Engine · Audit Logs · Settings · Dashboard (shell) · API Platform (gateway/versioning concerns) · File Management · Search (platform-wide) · Scheduler · License Management · IntegrationHub (connector framework).

### Inventory Management
SKU/Products · Variants · Batch tracking · Serial tracking · Barcode/QR · Inventory Ledger · Transfers · Reservations · Cycle Count · Stock Adjustment · Inventory Valuation (FIFO, Weighted Average Cost).

### Warehouse Management (WMS)
Multiple warehouses · Zones/Rack/Shelf/Bin · Receiving · Put-away · Picking · Packing · Dispatch · Cross-docking · Barcode scanning · RF scanner support · Warehouse dashboard.

### Procurement
RFQ · Supplier comparison · Purchase orders · Approvals · Goods receipt · Vendor returns · Vendor rating · Purchase history · Price history · Contracts.

### Sales
Quotation · Sales order · Invoice · Dispatch · Returns · Credit notes · Customer portal · Price lists · Discount rules.

### Manufacturing
Bill of Materials (BOM) · BOM versions · Raw materials · Alternative materials · Production orders · Work orders · MRP · Routing · Machine planning · Capacity planning · Material issue · Material return · WIP · Scrap · Rework · Yield management · Production dashboard.

### Finance
General Ledger · Accounts Payable · Accounts Receivable · GST/Taxes · Cost centers · Budgets · Assets · Bank · Financial reports.

### CRM
Leads · Pipeline · Customers · Activities · Meetings · Tasks · Sales forecast · Communication history.

### HRMS
Attendance · Leave · Payroll · Recruitment · Performance · Training · Employee records.

### Quality Management
Incoming inspection · In-process inspection · Finished goods inspection · CAPA · Non-conformance · Quality reports.

### Maintenance
Machine register · Preventive maintenance · Breakdown maintenance · Spare parts · Maintenance history.

### Business Intelligence
Dashboards · KPIs · Reports · Forecasts · Charts · Analytics · Export.

### AI Platform
FusionOS AI Copilot · Demand forecasting · Inventory optimization · Purchase recommendation · Production planning · OCR/invoice processing · Natural-language search · Report generation · Predictive analytics · Root-cause analysis. (Full detail: `12_AI_PLATFORM.md`.)

### Marketplace
Plugins · Themes · Report packs · Workflow packs · Industry extensions · AI agents.

### Mobile Applications
Warehouse app · Sales app · Management app · Approvals app · Production app · Maintenance app — all consuming the same `/api/v1/` contracts (`03_SYSTEM_ARCHITECTURE.md` §9).

## 2. Module Dependency Map (high level)

Core has no dependencies and is the foundation every other module depends on for identity, audit, notifications, and search. Inventory depends only on Core. Warehouse depends on Core + Inventory (via events, not direct calls). Procurement and Sales depend on Core + Inventory. Manufacturing depends on Core + Inventory + Warehouse (material movements) and publishes events Finance and Quality consume. Finance depends on Core and consumes events from nearly every transactional module (Procurement, Sales, Manufacturing, Inventory, HRMS/Payroll) but is never called synchronously by them. CRM depends on Core and Sales (read models). HRMS depends on Core. Quality and Maintenance depend on Core + Manufacturing/Inventory. Business Intelligence and AI are consumers of events/read-models from every module and must never be a dependency of any transactional module (a BI/AI outage must never block a sale, receipt, or production order). Marketplace/IntegrationHub sit alongside Core as extension frameworks used by all modules.

## 3. Phased Delivery Roadmap

The roadmap sequences by (a) dependency order above and (b) which modules unlock the earliest real customer value (trading/distribution companies before deep manufacturing, since trading ERP is the faster time-to-value wedge per the PRD's "fast implementation" goal).

**Phase 0 — Platform Foundation**
Core Platform in full: authentication, RBAC, organizations/companies/branches/departments, audit logs, notifications, settings, dashboard shell, API platform/versioning, file management, platform search, scheduler, license management. Nothing else is built before this phase is solid — every later module depends on it.

**Phase 1 — Trading ERP Core**
Inventory Management, Warehouse Management (core flows: receiving, put-away, picking, packing, dispatch), Procurement, Sales. This phase alone lets a trading/distribution/wholesale customer run end-to-end (the PRD's fastest-value target customer segment).

**Phase 2 — Financial Backbone**
Finance in full (GL, AP, AR, GST/Taxes, cost centers, bank), wired to consume events from Phase 1 modules. A platform is not commercially viable without this phase regardless of how complete Phase 1 is.

**Phase 3 — Manufacturing ERP**
BOM/BOM versions, production orders/work orders, MRP, routing, capacity planning, WIP/scrap/rework/yield. Unlocks the manufacturing customer segment and industries (automobile, FMCG, chemical, pharma, textile, etc.).

**Phase 4 — CRM & HRMS**
CRM (leads through forecast) and HRMS (attendance, leave, payroll, recruitment, performance, training) — broaden the platform from operations-only to full business operating system.

**Phase 5 — Quality & Maintenance**
Quality Management and Maintenance modules, deepening manufacturing-industry fit (regulated industries: pharma, medical, food & beverage particularly need Quality/CAPA).

**Phase 6 — Business Intelligence**
Cross-module dashboards, KPIs, forecasts, analytics, export — deliberately sequenced after enough modules exist to have meaningful cross-domain data to analyze.

**Phase 7 — AI Platform**
Forecasting, procurement recommendations, inventory optimization, production planning assistance, OCR, natural-language search, automated report generation, predictive analytics, root-cause analysis, and the AI Copilot surface. Sequenced after BI because AI features consume the same event/read-model backbone BI establishes. Detailed in `12_AI_PLATFORM.md`.

**Phase 8 — Marketplace & Ecosystem**
Plugin framework hardening, themes, report packs, workflow packs, industry extensions, third-party AI agents — opened once core modules and their extension points are stable enough to commit to as a public plugin contract.

**Phase 9 — Integrations & Mobile**
IntegrationHub connectors (Shopify, WooCommerce, Amazon, Flipkart, ONDC, Shiprocket, Delhivery, Razorpay, Stripe, WhatsApp, Email) and the six mobile applications. Pulled forward opportunistically wherever a specific customer/channel need justifies it — these are additive to the core and don't block on Phase 8 completing entirely.

**Phase 10 — Migration Tooling**
Migration paths from SAP Business One, SAP S/4HANA, Oracle, Microsoft Dynamics, Odoo, ERPNext, Tally, Busy, Zoho, Excel/CSV, and SQL sources. Detailed in `11_SAP_MIGRATION.md`. Built incrementally alongside Phases 1–3 in practice (a trading customer migrating from Tally needs migration tooling as soon as Phase 1 is usable), but formalized as its own workstream here so it is never treated as an afterthought.

## 4. Definition of "Module Complete"

A module is complete only when: its domain model and schema are reviewed against `04_DATABASE_GUIDELINES.md`; its API is documented and reviewed against `08_API_STANDARDS.md`; its frontend meets `06_UI_UX_DESIGN_SYSTEM.md`; unit and integration test coverage meets the bar in `09_CODING_STANDARDS.md`; its events are published/consumed per `03_SYSTEM_ARCHITECTURE.md` §4; and it has passed a security review (`07_SECURITY.md`) and a performance review against the targets in `09_CODING_STANDARDS.md`. Partial delivery against this list is tracked explicitly as "in progress," never presented as "done."
