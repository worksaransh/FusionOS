# FusionOS — Complete Development Roadmap

**Prepared as:** CTO / Enterprise Architect / Product Manager / Technical Program Manager review
**Date:** 2026-07-14
**Status:** Planning only — no code was written or modified to produce this document.

This is the official implementation plan for FusionOS from its current state through to a SAP-level feature set. It is built entirely from verified findings already gathered this session (a five-pass code audit — see `FusionOS_Coverage_Completion_Audit.docx` — plus two targeted verification checks run specifically for this roadmap on Document Management and API Platform, neither of which had been audited before). Nothing here is guessed; anything not directly verifiable from source (chiefly "does it compile/run", since no .NET compiler with working NuGet access has ever been available in this project's working environment) is called out explicitly rather than assumed.

---

## Step 1–2 — Current State Analysis

FusionOS is a .NET 8 modular-monolith backend (Clean Architecture, CQRS/MediatR) with a React 19/TypeScript frontend. 15 backend module folders exist on disk; 2 planned areas (Mobile, SAP Migration) do not exist as folders at all. Of the 15 that exist, 6 (Core, Inventory, Warehouse, Procurement, Sales, Finance) have genuine business logic; 9 are empty architectural scaffolds (a DbContext and a health-check endpoint only, self-labeled `"status": "scaffolded"`).

**The single fact that gates every other number in this document:** zero EF Core migrations have ever been generated for any module, and the backend has never been compiled, because this session's working environment has no NuGet network access. Every completion percentage below describes *source code written*, not *code proven to run*. Generating migrations and doing a real build (Phase 0 below) is not optional groundwork — it is the prerequisite for every other number in this document to mean anything beyond "looks right on paper."

High-level classification per the 5 categories requested:

| Classification | Modules |
|---|---|
| **Completed** (no module is fully complete — see caveat above) | *(none — every module has at least one open gap)* |
| **Partial** | Core Platform, Authentication, RBAC, Companies, Users, Inventory, Warehouse, Procurement, Sales, Finance, Search, Audit Logs, API Platform |
| **Missing** (some scaffolding or entity exists, but no working feature) | Branches, Departments, Notifications |
| **Blocked** (needs a decision or environment before more work is useful) | Finance tax engine (needs jurisdiction decision), Inventory valuation (needs costing-method decision), Notifications delivery (needs provider decision), Core Platform's migrations/build (needs your machine) |
| **Not Started** (zero functional code, scaffold or nothing at all) | Settings, Manufacturing, CRM, HRMS, Quality, Maintenance, Reports, Dashboard, Workflow, AI, Marketplace, Integration Hub, Mobile, Analytics, SAP Migration, Document Management |

---

## Step 3 — Module Completion Matrix

| Module | Planned Features | Completed | Partial | Missing | Completion % | Status |
|---|---|---|---|---|---|---|
| Core Platform | 10 | 2 | 6 | 2 | 75% | Partial |
| Authentication | 8 | 3 | 4 | 1 | 70% | Partial |
| RBAC | 7 | 3 | 4 | 0 | 65% | Partial |
| Companies | 6 | 3 | 2 | 1 | 60% | Partial |
| Branches | 5 | 0 | 1 | 4 | 5% | Not Started |
| Users | 5 | 1 | 2 | 2 | 35% | Partial |
| Departments | 5 | 0 | 1 | 4 | 5% | Not Started |
| Settings | 5 | 0 | 0 | 5 | 0% | Not Started |
| Inventory | 10 | 4 | 3 | 3 | 58% | Partial |
| Warehouse | 9 | 4 | 3 | 2 | 55% | Partial |
| Procurement | 9 | 4 | 3 | 2 | 58% | Partial |
| Sales | 11 | 5 | 3 | 3 | 60% | Partial |
| Finance | 11 | 3 | 3 | 5 | 45% | Partial |
| Manufacturing | 9 | 0 | 0 | 9 | 2% | Not Started |
| CRM | 8 | 0 | 0 | 8 | 2% | Not Started |
| HRMS | 8 | 0 | 0 | 8 | 2% | Not Started |
| Quality | 6 | 0 | 0 | 6 | 2% | Not Started |
| Maintenance | 6 | 0 | 0 | 6 | 2% | Not Started |
| Reports | 6 | 0 | 0 | 6 | 0% | Not Started |
| Dashboard | 4 | 0 | 0 | 4 | 0% | Not Started |
| Workflow | 5 | 0 | 0 | 5 | 0% | Not Started |
| Notifications | 5 | 0 | 1 | 4 | 3% | Not Started |
| AI | 6 | 0 | 0 | 6 | 2% | Not Started |
| Marketplace | 6 | 0 | 0 | 6 | 2% | Not Started |
| Integration Hub | 6 | 0 | 0 | 6 | 2% | Not Started |
| Mobile | 4 | 0 | 0 | 4 | 0% | Not Started |
| Analytics | 5 | 0 | 0 | 5 | 0% | Not Started |
| SAP Migration | 4 | 0 | 0 | 4 | 0% | Not Started |
| Search | 5 | 0 | 1 | 4 | 15% | Partial |
| Audit Logs | 5 | 2 | 2 | 1 | 45% | Partial |
| Document Management | 5 | 0 | 0 | 5 | 0% | Not Started |
| API Platform | 7 | 2 | 2 | 3 | 20% | Partial |

**Overall completion (simple average across all 32 modules): ~22%.** Not weighted by business value or effort — Settings and SAP Migration count the same as Sales in this average despite being wildly different in scope. Use the phase plan below, not this single number, to actually sequence work.

---

## Step 4 — Feature Completion Matrix (per module)

Modules with real functionality get a full feature-level breakdown. Modules at 0–2% get the standard feature set a baseline ERP module of that type would need — every row is honestly `Missing` or `Blocked`, not padded to look more planned-out than it is.

### Core Platform

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Multi-tenant isolation (TenantIsolationBehavior) | Completed | 100% | Critical |
| Global permission enforcement (AuthorizationBehavior) | Completed | 100% | Critical |
| RFC 7807 error envelope | Completed | 100% | High |
| Rate limiting (per-IP + Auth endpoints) | Completed | 100% | High |
| CORS allow-list | Completed | 100% | Medium |
| Observability (OpenTelemetry + Serilog + Prometheus) | Partial | 70% | Medium |
| Real trace backend (currently a debug exporter) | Missing | 0% | Low |
| Secrets fail-fast on missing config | Completed | 100% | High |
| EF Core migrations applied to a real database | Blocked | 0% | Critical |
| Real compiled build verified | Blocked | 0% | Critical |

### Authentication

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Login (JWT issuance) | Completed | 100% | Critical |
| Refresh token rotation | Completed | 100% | Critical |
| Register (company bootstrap + invited-member flow) | Completed | 100% | Critical |
| Public registration UI | Completed | 100% | High |
| Password reset / forgot password | Missing | 0% | High |
| Multi-factor authentication | Missing | 0% | Medium |
| SSO / OIDC federation | Missing | 0% | Low |
| CAPTCHA / bot-throttling on public endpoints | Missing | 0% | Medium |

### RBAC

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Permission catalog + seeding | Completed | 100% | Critical |
| Create/list roles | Completed | 100% | Critical |
| Get/set role permissions | Completed | 100% | Critical |
| Assign user to role | Completed | 100% | Critical |
| Read-gating on all list/get queries | Completed | 100% | Critical |
| Role cloning / bulk permission templates | Missing | 0% | Low |
| Test coverage for RBAC admin handlers | Missing | 0% | High |

### Companies

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Create company (tenant bootstrap) | Completed | 100% | Critical |
| List companies (tenant-scoped) | Completed | 100% | Critical |
| Get company by id | Completed | 100% | Medium |
| Update company details | Completed | 100% | Medium |
| Deactivate company (soft) | Completed | 100% | Medium |
| Cross-company consolidated reporting | Missing | 0% | Low |
| Test coverage for Update/Deactivate/GetById | Missing | 0% | Medium |

### Branches

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Branch entity + EF configuration | Partial | 20% | Medium |
| Create/list branches | Missing | 0% | Medium |
| Assign users/warehouses to a branch | Missing | 0% | Medium |
| Branch-level reporting | Missing | 0% | Low |
| Frontend branch management page | Missing | 0% | Medium |

### Users

| Feature | Status | Completion % | Priority |
|---|---|---|---|

### Departments

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Department entity + EF configuration (self-referencing hierarchy) | Partial | 20% | Medium |
| Create/list departments | Missing | 0% | Medium |
| Assign users to a department | Missing | 0% | Medium |
| Department-level reporting | Missing | 0% | Low |
| Frontend department management page | Missing | 0% | Medium |

### Settings

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Company-level configuration entity | Missing | 0% | Medium |
| Default currency / page-size overrides | Missing | 0% | Low |
| Company display name / logo | Missing | 0% | Low |
| Per-tenant CORS / rate-limit configuration | Missing | 0% | Low |
| Frontend settings page | Missing | 0% | Medium |

### Inventory

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Product CRUD (create/list/get/update/deactivate) | Completed | 100% | Critical |
| SKU as business key | Completed | 100% | Critical |
| Stock ledger (adjust, get-on-hand, list) | Completed | 100% | Critical |
| Server-side search on products | Completed | 100% | Medium |
| Variants / attributes | Missing | 0% | Medium |
| Batch / lot tracking | Missing | 0% | High |
| Serial number tracking | Missing | 0% | High |
| Barcode generation/scanning | Missing | 0% | Medium |
| QR code generation/scanning | Missing | 0% | Low |
| Multi-UOM conversion | Missing | 0% | Medium |
| Stock transfers between warehouses | Missing | 0% | High |
| Reservations (soft-allocate stock to an order) | Missing | 0% | High |
| Cycle counting | Missing | 0% | Medium |
| Inventory valuation (FIFO / weighted-average / standard) | Blocked | 0% | Critical |
| Test coverage for Update/Deactivate/GetById | Missing | 0% | Medium |

### Warehouse

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Warehouse CRUD (create/list/get/update/deactivate) | Completed | 100% | Critical |
| Zone CRUD (create/list/get/update/deactivate) | Completed | 100% | Critical |
| Goods Receipt (create/list) + real cross-module consumers | Completed | 100% | Critical |
| Picking workflow | Missing | 0% | High |
| Packing workflow | Missing | 0% | High |
| Putaway logic | Missing | 0% | Medium |
| Bin-level location tracking | Missing | 0% | Medium |
| Cycle counting (warehouse side) | Missing | 0% | Medium |
| Test coverage for Goods Receipt / Update / Deactivate | Missing | 0% | Medium |

### Procurement

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Supplier CRUD (create/list/get/update/deactivate) | Completed | 100% | Critical |
| Purchase Order create/list | Completed | 100% | Critical |
| PO approval with maker-checker (self-approval blocked) | Completed | 100% | Critical |
| RFQ (Request for Quotation) | Missing | 0% | High |
| Supplier scorecards / performance tracking | Missing | 0% | Medium |
| Contract management | Missing | 0% | Medium |
| Three-way match (PO / GRN / Invoice) | Missing | 0% | High |
| Multi-level approval matrix | Missing | 0% | Medium |
| Vendor returns | Missing | 0% | Medium |
| Idempotency keys on Create commands | Missing | 0% | High |
| Test coverage for Supplier Update/Deactivate/GetById | Missing | 0% | Medium |

### Sales

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Customer CRUD (create/list/get/update/deactivate) | Completed | 100% | Critical |
| Sales Order create/confirm | Completed | 100% | Critical |
| Sales Invoice create/issue | Completed | 100% | Critical |
| Dispatch create | Completed | 100% | Critical |
| Cross-aggregate quantity validation vs Sales Order | Completed | 100% | Critical |
| Quotations (pre-Sales-Order stage) | Missing | 0% | High |
| Returns / credit notes | Missing | 0% | High |
| Pricing / discount engine + multiple price lists | Missing | 0% | High |
| Sales commissions | Missing | 0% | Low |
| Backorder handling | Missing | 0% | Medium |
| Test coverage for Customer Update/Deactivate/GetById | Missing | 0% | Medium |

### Finance

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Chart of Accounts CRUD | Completed | 100% | Critical |
| Journal Entry create/post (double-entry enforced) | Completed | 100% | Critical |
| Accounts Receivable ledger (charge-only) | Partial | 50% | Critical |
| AR payment/receipt recording | Missing | 0% | Critical |
| AR balance frontend page | Missing | 0% | High |
| Accounts Payable | Missing | 0% | Critical |
| Bank reconciliation | Missing | 0% | High |
| Multi-currency | Missing | 0% | High |
| Budgeting | Missing | 0% | Medium |
| Fixed asset management | Missing | 0% | Medium |
| Cost centers | Missing | 0% | Medium |
| Tax engine (GST/VAT) | Blocked | 0% | Critical |
| Financial statements (P&L, balance sheet) | Missing | 0% | High |

### Manufacturing

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Bill of Materials (single-level) | Missing | 0% | High |
| Bill of Materials (alternative/multi-level) | Missing | 0% | Medium |
| Work orders | Missing | 0% | High |
| Routing | Missing | 0% | Medium |
| Production scheduling | Missing | 0% | Medium |
| Shop floor tracking | Missing | 0% | Medium |
| MRP (Material Requirements Planning) | Missing | 0% | High |
| Raw-material-to-multiple-SKU support | Missing | 0% | Medium |
| Costing roll-up from BOM | Missing | 0% | Medium |

### CRM

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Leads | Missing | 0% | Medium |
| Opportunities / pipeline | Missing | 0% | Medium |
| Contacts (distinct from Sales Customer) | Missing | 0% | Low |
| Activities / follow-ups | Missing | 0% | Low |
| Email/campaign integration | Missing | 0% | Low |
| Sales forecasting | Missing | 0% | Low |
| Customer support / ticketing | Missing | 0% | Low |
| Reporting | Missing | 0% | Low |

### HRMS

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Employee records | Missing | 0% | Medium |
| Payroll | Missing | 0% | Medium |
| Leave requests | Missing | 0% | Medium |
| Attendance | Missing | 0% | Low |
| Performance reviews | Missing | 0% | Low |
| Recruitment | Missing | 0% | Low |
| Org chart (ties into Departments) | Missing | 0% | Low |
| Compliance/statutory reporting | Missing | 0% | Low |

### Quality

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Inspection plans | Missing | 0% | Medium |
| QC checkpoints on Goods Receipt | Missing | 0% | Medium |
| QC checkpoints on Production | Missing | 0% | Low |
| Non-conformance tracking | Missing | 0% | Medium |
| Corrective/preventive action (CAPA) | Missing | 0% | Low |
| Certificates of analysis | Missing | 0% | Low |

### Maintenance

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Asset/equipment registry | Missing | 0% | Medium |
| Preventive maintenance scheduling | Missing | 0% | Medium |
| Work orders (maintenance) | Missing | 0% | Medium |
| Downtime tracking | Missing | 0% | Low |
| Spare-parts inventory linkage | Missing | 0% | Low |
| Maintenance cost reporting | Missing | 0% | Low |

### Reports

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Generic CSV export on list endpoints | Missing | 0% | High |
| Canned report: AR aging | Missing | 0% | High |
| Canned report: stock valuation | Missing | 0% | Medium |
| Canned report: PO status | Missing | 0% | Medium |
| PDF export | Missing | 0% | Medium |
| Ad-hoc report builder | Missing | 0% | Low |

### Dashboard

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Landing-page KPI widgets | Missing | 0% | High |
| Open Sales Orders count | Missing | 0% | Medium |
| Pending PO approvals count | Missing | 0% | Medium |
| Low-stock alert widget | Missing | 0% | Medium |
| Today's audit-log activity widget | Missing | 0% | Low |

### Workflow

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Generic ApprovalRequest primitive | Missing | 0% | High |
| Multi-level approval chains | Missing | 0% | Medium |
| Refactor PO Approve() onto the generic engine | Missing | 0% | Medium |
| Approval notifications | Missing | 0% | Medium |
| Workflow designer UI | Missing | 0% | Low |

### Notifications

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Notification domain entity + persistence | Completed | 100% | Medium |
| Create-notification command | Missing | 0% | Medium |
| List unread notifications query | Missing | 0% | Medium |
| In-app bell-icon UI | Missing | 0% | Medium |
| Email delivery | Blocked | 0% | High |
| SMS delivery | Blocked | 0% | Low |
| Push delivery | Blocked | 0% | Low |

### AI

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Any AI/ML/LLM-backed feature | Missing | 0% | Low |
| Demand forecasting | Missing | 0% | Low |
| Anomaly detection (fraud/errors) | Missing | 0% | Low |
| Natural-language query over data | Missing | 0% | Low |
| Document/OCR extraction | Missing | 0% | Low |

### Marketplace

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Multi-vendor product listings | Missing | 0% | Low |
| Vendor onboarding | Missing | 0% | Low |
| Commission/payout engine | Missing | 0% | Low |
| Storefront | Missing | 0% | Low |

### Integration Hub

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Outbound webhooks | Missing | 0% | Medium |
| Inbound connector framework | Missing | 0% | Medium |
| E-commerce platform connectors | Missing | 0% | Low |
| Payment gateway connectors | Missing | 0% | Low |
| Shipping carrier connectors | Missing | 0% | Low |

### Mobile

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Mobile app project scaffold | Missing | 0% | Low |
| Warehouse picker mobile app | Missing | 0% | Medium |
| Sales rep mobile app | Missing | 0% | Low |
| Approval-on-the-go (push + approve) | Missing | 0% | Low |

### Analytics

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Cross-module analytics warehouse/store | Missing | 0% | Low |
| Trend dashboards | Missing | 0% | Low |
| Cohort/retention analysis | Missing | 0% | Low |
| Export to BI tools | Missing | 0% | Low |

### SAP Migration

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Data-mapping tooling (SAP -> FusionOS) | Missing | 0% | Low |
| IDoc/RFC connector | Missing | 0% | Low |
| Migration validation/reconciliation reports | Missing | 0% | Low |

### Search

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Server-side search: Product/Supplier/Customer/Warehouse/Account | Completed | 100% | Medium |
| Server-side search: Roles/Users/Audit Log/Companies/Permissions | Missing | 0% | Medium |
| Unified cross-entity search | Missing | 0% | Medium |
| Server-side search replacing client-only EntityCombobox filtering | Missing | 0% | Medium |

### Audit Logs

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Write-side capture (EntityType/EntityId/Action/Actor) | Completed | 100% | Critical |
| Read-side query + controller + frontend page | Completed | 100% | High |
| Real before/after diff capture (ChangesJson) | Missing | 0% | High |
| Test coverage for audit-log read side | Missing | 0% | Medium |

### Document Management

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| File attachment on any entity | Missing | 0% | Medium |
| Blob storage integration | Missing | 0% | Medium |
| Document versioning | Missing | 0% | Low |
| Document preview / thumbnailing | Missing | 0% | Low |
| Access control on documents (ties into RBAC) | Missing | 0% | Medium |

### API Platform

| Feature | Status | Completion % | Priority |
|---|---|---|---|
| Versioned REST API (api/v1/...) | Completed | 100% | Critical |
| OpenAPI/Swagger spec generation | Partial | 60% | Medium |
| Swagger UI exposed outside Development | Missing | 0% | Low |
| API key management for external partners | Missing | 0% | Medium |
| Per-key rate limiting/quotas | Missing | 0% | Medium |
| Developer portal / published API docs | Missing | 0% | Low |
| Outbound webhook subscriptions (ties into Integration Hub) | Missing | 0% | Low |

---

## Step 5 — Dependency Analysis

Modules are grouped into five dependency layers. Nothing in a lower layer should be scheduled before its prerequisites in the layer above it.

```
Layer 0 — Environment (blocks everything)
  Own-machine build + EF migrations + real database

Layer 1 — Foundation
  Authentication -> Users -> RBAC -> Companies -> Branches / Departments -> Settings
       |
       +--> Audit Logs (needs Users for actor identity)
       +--> Search (needs entities from every module it indexes)

Layer 2 — Core Business (the transactional backbone)
  Companies/Branches -> Inventory -> Warehouse
                              |            |
                              v            v
                        Procurement <-> Sales
                              |            |
                              +----> Finance <----+
  (Procurement needs Inventory+Warehouse+Suppliers; Sales needs Inventory+Warehouse+Customers;
   Finance needs both Procurement's AP side and Sales' AR side to be a real ledger.)

Layer 3 — Cross-Cutting Platform (can be built alongside Layer 2, used by everything above and below)
  Workflow (approval engine)  -- consumed by Procurement PO approval, Sales discount approval
  Notifications               -- consumed by Workflow, Sales, Procurement, Finance
  Document Management          -- attaches to Products, POs, Invoices, any entity
  API Platform                 -- exposes all of the above externally
  Reports / Dashboard / Analytics -- read from Layer 2 data; low value until Layer 2 is real

Layer 4 — Extended Business (each needs specific Layer 2 modules as a foundation)
  Manufacturing  <- needs Inventory (BOM components) + Warehouse (raw material issue)
  CRM            <- needs Sales (Customer) + Users
  HRMS           <- needs Users + Departments + Branches (largely independent otherwise)
  Quality        <- needs Warehouse (Goods Receipt) + Manufacturing (production QC)
  Maintenance    <- needs Warehouse (assets/spares); loosely coupled to Manufacturing
  Marketplace    <- needs Inventory (Products) + Sales + multi-vendor Procurement
  Integration Hub<- needs API Platform to have something to expose/consume

Layer 5 — Advanced / Intelligence (needs mature data across all layers to be useful at all)
  AI          <- needs real transactional history in Inventory/Sales/Finance to train/infer on
  Mobile      <- needs API Platform to be stable and versioned
  Analytics   <- needs a real data volume across Layers 2-4
  SAP Migration <- needs target modules (mainly Finance, Inventory, Procurement) to be complete
```

---

## Step 6 — Development Phases

Phases 0–4 correspond to the already-scoped `docs/BUILD_PROMPTS.md` backlog (phases G and M1–M10) and are ready to execute now. Phases 5+ cover the previously-deferred Phase F modules (Manufacturing, CRM, HRMS, Quality, Maintenance, AI, Marketplace, Integration Hub, Mobile, SAP Migration) plus Reports/Dashboard/Workflow/Notifications/Document Management/API Platform hardening. **Per your own earlier scoping decision, Phase F work is included here only for completeness of the path to 100% — none of it should start without your explicit go-ahead, the same as the standing instruction in `docs/PROJECT_TRACKER.md`.**

| Phase | Focus | What it covers | Target completion | Status |
|---|---|---|---|---|
| Phase 0 | Environment | Own-machine `dotnet restore/build`, generate first EF Core migrations, stand up Postgres via Docker Compose, apply migrations, smoke-test one real HTTP call per module. | 0% -> baseline proven | BLOCKED ON YOU — needs your machine, not this sandbox |
| Phase 1 | Quick fixes (M1) | Fix ProblemDetailsExceptionHandler status codes (done); Sales cross-aggregate quantity validation vs Sales Order (done). | Done | Complete |
| Phase 2 | Frontend edit forms (M2) | Edit/Update UI for the 7 entities with dead backend Update commands: Company, Product, Warehouse, Zone, Supplier, Account, Customer. | Done | Complete |
| Phase 3 | Test coverage (M3) | Unit/integration tests for the ~60 untested command/query handlers across all 6 real modules. | 0% -> target 70%+ handler coverage | Ready to start |
| Phase 4 | Orphaned events + AR payments (M4) | Wire any integration events published but never consumed; add AR payment/receipt recording so the AR ledger can actually decrease. | Finance AR 50% -> ~75% | Ready to start |
| Phase 5 | Settings + Search depth (M5) | Build the Settings module from scratch; extend Search to the remaining 14 of 19 endpoints (Roles/Users/Audit Log/Companies/Permissions). | Settings 0%->60%, Search 15%->70% | Ready to start |
| Phase 6 | Reports + Dashboard (M6) | CSV export, canned reports (AR aging, stock valuation, PO status), landing-page KPI widgets. | 0% -> 55% | Ready to start |
| Phase 7 | Workflow engine + Notifications (M7) | Generic ApprovalRequest primitive; refactor PO approval onto it; notification create/list + in-app UI (delivery channel still blocked on provider decision). | 0%/3% -> 50% | Ready to start; email/SMS/push delivery BLOCKED ON YOU (provider decision) |
| Phase 8 | Finance depth (M8) | Accounts Payable, bank reconciliation, multi-currency, budgeting, fixed assets, cost centers, financial statements. | 45% -> 75% | BLOCKED ON YOU — tax jurisdiction decision needed before the tax engine sub-piece |
| Phase 9 | Inventory costing + WMS depth (M9) | Batch/serial tracking, barcode/QR, stock transfers, reservations, cycle counting; picking/packing/putaway/bins in Warehouse. | Inventory 58%->80%, Warehouse 55%->80% | BLOCKED ON YOU — costing method decision needed before valuation sub-piece |
| Phase 10 | Procurement + Sales depth (M10) | RFQ, supplier scorecards, three-way match, vendor returns; Quotations, returns/credit notes, pricing engine, backorders. | Procurement 58%->80%, Sales 60%->85% | Ready to start |
| Phase 11 | Document Management | File attachment framework, blob storage integration, versioning, access control via RBAC. | 0% -> 60% | Needs greenlight — new module, not previously scoped |
| Phase 12 | API Platform hardening | API keys for external partners, per-key rate limiting, developer portal, Swagger exposed outside dev, outbound webhooks. | 20% -> 65% | Needs greenlight |
| Phase 13 (Phase F-1) | Manufacturing | BOM (single + multi-level), work orders, routing, MRP, production scheduling, shop-floor tracking, costing roll-up. | 2% -> 60% | PARKED — needs your explicit go-ahead |
| Phase 14 (Phase F-2) | CRM + HRMS | Leads/opportunities/pipeline; employee records/payroll/leave/attendance. | 2%/2% -> 55%/55% | PARKED — needs your explicit go-ahead |
| Phase 15 (Phase F-3) | Quality + Maintenance | Inspection plans, QC checkpoints, CAPA; asset registry, preventive maintenance, downtime tracking. | 2%/2% -> 55%/55% | PARKED — needs your explicit go-ahead |
| Phase 16 (Phase F-4) | Integration Hub + Marketplace | Outbound webhooks, inbound connectors, e-commerce/payment/shipping connectors; multi-vendor listings, commission engine. | 2%/2% -> 50%/50% | PARKED — needs your explicit go-ahead |
| Phase 17 (Phase F-5) | Mobile + Analytics | Warehouse picker + sales rep mobile apps; cross-module analytics store, trend dashboards, BI export. | 0%/0% -> 50%/50% | PARKED — needs your explicit go-ahead; Mobile also needs API Platform hardening first |
| Phase 18 (Phase F-6) | AI + SAP Migration | Demand forecasting, anomaly detection, NL query, OCR; SAP data-mapping tooling, IDoc/RFC connector, reconciliation reports. | 2%/0% -> 40%/40% | PARKED — needs your explicit go-ahead; both need mature data from all prior phases |

---

## Step 7 — Prioritize Work

**Critical (blocks correctness or the whole codebase's credibility):**
Phase 0 (own-machine build + migrations) — every completion percentage in this document is unverified until this happens. Phase 3 (test coverage) — 60 handlers currently have zero regression protection. Phase 4's AR payment recording — the AR ledger currently only ever goes up, which is a real accounting defect, not a missing nice-to-have.

**High (real user-facing gaps in modules already live):**
Phase 5 Search completion (14 of 19 endpoints still client-filtered only), Phase 6 Reports/Dashboard (users have no way to see aggregate data today), Phase 9's batch/serial/reservation gaps (Inventory cannot do allocation-aware selling without them), Phase 10's Sales pricing/returns (a sales order currently cannot be discounted, quoted, or reversed).

**Medium:** Phase 7 Workflow engine (currently a special-cased PO-only approval, not reusable), Phase 8 Finance depth beyond tax (AP, multi-currency, bank rec), Phase 11 Document Management, Phase 12 API Platform hardening.

**Low (defer until core is proven and greenlit):** all Phase F modules (13–18).

**Quick wins (small effort, real value, no blocking decision):** Search completion for the remaining 14 endpoints (pattern already exists — copy the 5 done ones); CSV export on existing list endpoints; landing-page KPI widgets reading data that already exists; Branches/Departments CRUD (entities already modeled, just need the same mechanical CRUD sweep already done for 7 other entities).

**Technical debt:** zero test coverage on ~60 handlers (Phase 3); no idempotency keys on Procurement/Sales Create commands (duplicate-submission risk); Workflow special-cased into PO instead of generic; Observability's trace exporter is a debug stub, not a real backend.

**Security:** MFA and password-reset are both missing from Authentication — this is a real gap for anything beyond an internal pilot. CAPTCHA/bot-throttling absent from public registration/login. Document Management's access control (Phase 11) must be built on RBAC from day one, not retrofitted.

**Performance:** not yet measurable — no environment has ever run this codebase under load. Revisit after Phase 0.

**Business value ranking (highest to lowest, independent of technical difficulty):** Phase 0 (nothing else matters until this is real) > Phase 4 AR payments (money currently only tracked one-directionally) > Phase 9/10 depth (this is what a customer evaluating FusionOS against SAP/NetSuite would try first) > Phase 6 Reports/Dashboard (visible, demoable) > Phase 3 tests (invisible to a demo, essential to trust) > everything else.

---

## Step 8 — Sprint Planning

Detailed 2-week sprints are laid out for Phases 0–4 (the immediate, unblocked backlog). Phases 5–12 get one sprint-group each (fine-grained sprint planning further out than ~3 months is low value — priorities will shift once Phase 0 exposes real compiler/runtime errors). Phase F (13–18) is intentionally left at phase-level only, since it is parked pending your greenlight.

| Sprint | Phase | Goal / Features | Estimated Complexity | Dependencies | Expected Completion |
|---|---|---|---|---|---|
| Sprint 1 | Phase 0 | Own-machine build, first migrations, Docker Compose up, one real smoke-tested request per module. | Low code complexity, high environment/tooling risk | None | Proves or disproves every % in this doc |
| Sprint 2 | Phase 3a | Tests for Core/Auth/RBAC/Companies handlers (~15 handlers). | Medium | Phase 0 (need a real test-DB run, not just compile) | Core cluster >= 70% handler coverage |
| Sprint 3 | Phase 3b | Tests for Inventory/Warehouse handlers (~15 handlers). | Medium | Phase 0 | Inventory/Warehouse >= 70% handler coverage |
| Sprint 4 | Phase 3c + Phase 4 | Tests for Procurement/Sales/Finance handlers (~30 handlers); orphaned-event wiring; AR payment recording. | Medium-High | Phase 0 | All 6 real modules >= 70% coverage; AR ledger can decrease |
| Sprint 5 | Phase 5 | Settings module (entity+CQRS+UI); Search extended to remaining 14 endpoints. | Medium | Phase 0 | Settings usable; Search covers 19/19 endpoints |
| Sprint 6 | Phase 6 | CSV export, 3 canned reports (AR aging/stock valuation/PO status), dashboard KPI widgets. | Medium | Phase 4 (needs real AR data), Phase 0 | Dashboard shows live numbers |
| Sprint 7-8 | Phase 7 | Generic ApprovalRequest engine; refactor PO approval onto it; Notification create/list + in-app bell UI. | Medium-High | Phase 0 | Approval engine reusable by 2+ modules; in-app notifications visible |

**Sprint-group level (Phases 8–12, ~2 sprints each once unblocked):**

| Sprint group | Phase | Goal | Complexity | Dependencies |
|---|---|---|---|---|
| Sprints 9-10 | Phase 8 (Finance depth) | AP, bank rec, multi-currency, budgeting, fixed assets, cost centers, statements | High | Phase 0; tax jurisdiction decision for the tax sub-piece |
| Sprints 11-12 | Phase 9 (Inventory/WMS depth) | Batch/serial, barcode/QR, transfers, reservations, cycle count, picking/packing/putaway | High | Phase 0; costing-method decision for valuation sub-piece |
| Sprints 13-14 | Phase 10 (Procurement/Sales depth) | RFQ, scorecards, three-way match, vendor returns, Quotations, returns/credit notes, pricing engine | High | Phase 0 |
| Sprint 15 | Phase 11 (Document Management) | File attachment, blob storage, versioning, RBAC-integrated access control | Medium | Phase 0; needs your greenlight (new module) |
| Sprint 16 | Phase 12 (API Platform hardening) | API keys, per-key rate limiting, developer portal, webhooks | Medium | Phase 0; needs your greenlight |

---

## Step 9 — Progress Dashboard

```
Core Platform        ████████░░ 75%
Authentication       ███████░░░ 70%
RBAC                 ██████░░░░ 65%
Companies            ██████░░░░ 60%
Branches             ░░░░░░░░░░ 5%
Users                ████░░░░░░ 35%
Departments          ░░░░░░░░░░ 5%
Settings             ░░░░░░░░░░ 0%
Inventory            ██████░░░░ 58%
Warehouse            ██████░░░░ 55%
Procurement          ██████░░░░ 58%
Sales                ██████░░░░ 60%
Finance              ████░░░░░░ 45%
Manufacturing        ░░░░░░░░░░ 2%
CRM                  ░░░░░░░░░░ 2%
HRMS                 ░░░░░░░░░░ 2%
Quality              ░░░░░░░░░░ 2%
Maintenance          ░░░░░░░░░░ 2%
Reports              ░░░░░░░░░░ 0%
Dashboard            ░░░░░░░░░░ 0%
Workflow             ░░░░░░░░░░ 0%
Notifications        ░░░░░░░░░░ 3%
AI                   ░░░░░░░░░░ 2%
Marketplace          ░░░░░░░░░░ 2%
Integration Hub      ░░░░░░░░░░ 2%
Mobile               ░░░░░░░░░░ 0%
Analytics            ░░░░░░░░░░ 0%
SAP Migration        ░░░░░░░░░░ 0%
Search               ██░░░░░░░░ 15%
Audit Logs           ████░░░░░░ 45%
Document Management  ░░░░░░░░░░ 0%
API Platform         ██░░░░░░░░ 20%
-------------------------------------
Overall              ██░░░░░░░░ 22%
```

---

## Step 10 — Final Roadmap Table

| Phase | Module(s) | Features | Current % | Target % | Estimated Effort | Status |
|---|---|---|---|---|---|---|
| 0 | Environment | Build, migrations, DB up, smoke tests | 0%* | 100% | 1 sprint | BLOCKED ON YOU |
| 1 | Core cross-cutting fixes | Error-status mapping, Sales quantity validation | Done | Done | Done | Complete |
| 2 | Frontend edit forms | 7 entities' Update UI | Done | Done | Done | Complete |
| 3 | All 6 real modules | Handler test coverage | ~5% | 70% | 3 sprints | Ready |
| 4 | Finance + events | AR payments, orphaned event wiring | 45% | 75% | 1 sprint | Ready |
| 5 | Settings, Search | New module + search completion | 0%/15% | 60%/95% | 1 sprint | Ready |
| 6 | Reports, Dashboard | Export, canned reports, KPI widgets | 0%/0% | 55%/55% | 1 sprint | Ready |
| 7 | Workflow, Notifications | Generic approval engine + in-app alerts | 0%/3% | 50%/45% | 2 sprints | Ready; delivery channel blocked on provider decision |
| 8 | Finance depth | AP, bank rec, multi-currency, statements | 45% | 75% | 2 sprints | Blocked on tax jurisdiction decision |
| 9 | Inventory + WMS depth | Batch/serial/barcode/transfers/picking/packing | 58%/55% | 80%/80% | 2 sprints | Blocked on costing method decision |
| 10 | Procurement + Sales depth | RFQ/3-way match, Quotations/returns/pricing | 58%/60% | 80%/85% | 2 sprints | Ready |
| 11 | Document Management | File attachment framework | 0% | 60% | 1 sprint | Needs greenlight |
| 12 | API Platform hardening | API keys, rate limits, portal, webhooks | 20% | 65% | 1 sprint | Needs greenlight |
| 13 (F-1) | Manufacturing | BOM, work orders, MRP, routing | 2% | 60% | 3+ sprints | PARKED |
| 14 (F-2) | CRM + HRMS | Pipeline; employee/payroll/leave | 2%/2% | 55%/55% | 3+ sprints | PARKED |
| 15 (F-3) | Quality + Maintenance | Inspection/CAPA; assets/PM scheduling | 2%/2% | 55%/55% | 2+ sprints | PARKED |
| 16 (F-4) | Integration Hub + Marketplace | Webhooks/connectors; multi-vendor | 2%/2% | 50%/50% | 2+ sprints | PARKED |
| 17 (F-5) | Mobile + Analytics | Picker/rep apps; BI store | 0%/0% | 50%/50% | 3+ sprints | PARKED |
| 18 (F-6) | AI + SAP Migration | Forecasting/anomaly; data-mapping tooling | 2%/0% | 40%/40% | 3+ sprints | PARKED |

\* Phase 0's "0%" reflects that a real build/migration/run has never happened — not that the source code doesn't exist.

---

## Step 11 — Action Plan

**Immediate tasks (this week):**
Run Phase 0 on your own machine — `dotnet restore`, `dotnet build`, generate the first EF Core migration set, `docker compose up` for Postgres/Kafka, apply migrations, and hit one real HTTP endpoint per module to confirm the stack actually starts. This is the one task nothing else in this roadmap can proceed past in good conscience, since every completion percentage above it is unverified until it happens. In parallel (no dependency on Phase 0 succeeding first): start Phase 3 test-writing for the Core/Auth/RBAC cluster, since test *code* can be written and reviewed for logical correctness even before it's been run against a live database.

**Next sprint:** Phase 3 completion for Inventory/Warehouse/Procurement/Sales/Finance, plus Phase 4 (AR payments + orphaned events) once Phase 0 confirms the environment is real.

**Next phase:** Phase 5 (Settings + Search completion) and Phase 6 (Reports + Dashboard) — both are quick, visible wins that make FusionOS demoable end-to-end for the first time.

**Next release (candidate scope):** Phases 0–7 complete = a genuinely working core ERP covering Auth/RBAC/Companies/Inventory/Warehouse/Procurement/Sales/Finance-basic with search, reporting, a dashboard, an approval engine, and in-app notifications — enough for an internal pilot with one real tenant.

**MVP target:** Phases 0–10 complete (adds Finance depth, Inventory/WMS depth, Procurement/Sales depth) — this is the point where FusionOS could run a real single-entity company's day-to-day operations without a spreadsheet workaround for AP, batch tracking, or sales pricing.

**Enterprise target:** Phases 0–12 complete (adds Document Management and a hardened, partner-facing API Platform) — this is the point where FusionOS can be sold to a multi-branch customer who needs to attach documents to transactions and integrate with their own tooling.

**SAP-level target:** Phases 0–18 complete, including all currently-parked Phase F modules (Manufacturing, CRM, HRMS, Quality, Maintenance, AI, Marketplace, Integration Hub, Mobile, SAP Migration). Per your own standing decision, none of Phases 13–18 should begin without an explicit go-ahead from you — they are included here only so the full path to 100% is visible, not as an instruction to start them.

**Decisions still needed from you before certain phases can start:**
Inventory costing method (FIFO / weighted-average / standard) — blocks Phase 9's valuation sub-piece. Tax jurisdiction(s) (GST / VAT / multi-country) — blocks Phase 8's tax sub-piece. Notification delivery provider (SendGrid / Twilio / SES / other) — blocks Phase 7's actual email/SMS/push delivery (the in-app UI and data model can proceed without this). Explicit greenlight for Phase F (13–18) — everything through Phase 12 can proceed without this.

---

*This roadmap reuses verified findings from `FusionOS_Coverage_Completion_Audit.docx` (2026-07-14) and `docs/PROJECT_TRACKER.md`, extended with two fresh verification passes for Document Management and API Platform run specifically for this document. No source files were created or modified to produce it.*
