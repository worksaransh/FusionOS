# FusionOS Multi-Expert Audit Report — Luxor Writing Instruments Pilot

**Date:** 4 August 2026
**Method:** Live end-to-end run of FusionOS (backend on `localhost:5000`, frontend on `localhost:5173`, local PostgreSQL 18) against a real tenant — "Luxor Writing Instruments" — seeded with data researched from Luxor Pens' actual public business profile. Findings below are from direct interaction (browser DOM walkthrough + authenticated API calls), not code inspection alone. Screenshots were captured live during the session (Inventory/Products, Sales/Customers, HRMS/Employees, Finance/Chart-of-Accounts) and are reproduced inline in the session transcript; the same views are reproducible by any reviewer following the steps in Appendix A.

This report is written as a panel of domain experts, each auditing the area of the platform relevant to their specialty, against a real Luxor-style dataset.

---

## 0. About the seed data (source of truth)

Real data researched from public sources about Luxor Writing Instruments Pvt. Ltd. (founded 1963):

- Revenue ₹449 crore (FY ending 31 Mar 2025), 853 employees, 8 manufacturing facilities, 5 regional sales offices, >1.5 million retail outlets, ~6 million pens/day production, exports to 85+ countries, brands Parker/Schneider/Luxor/Doodles/Crayola. [Tracxn](https://tracxn.com/d/legal-entities/india/luxor-writing-instruments-private-limited/__ldBHVzJErZUDuUua_jQYelrL3BuAj3gFHDKfffp4s7I), [Roastbrief](https://roastbrief.us/luxor-to-showcase-its-complete-portfolio-of-writing-instruments-and-creative-solutions-at-paperworld-india-giftex-2026/)
- Real product line names used for seeding: Konark, Impact, TRIOS ICY, InkGlide, Spark Classic, Focus ICY, Stick, Spark II, Style, Roller 07, Xonox MX/PX, Micra, plus Parker Jotter and Crayola crayons (both real Luxor-distributed brands). [Moglix](https://www.moglix.com/brands/luxor/office-supplies/writing-correction/pens/214151100), [Luxor](https://luxor.in/pen.html)

Seeded into the running tenant via the real REST API (not fixtures): 1 company, 8 departments, 18 products/SKUs, 5 customers (domestic distributors + one export customer), 4 raw-material suppliers, 5 employees.

**Org-structure follow-up:** the tenant's actual reporting structure was modeled as 4 real Core roles, with the hierarchy corrected mid-session per the client's own framing — **Super Admin owns the software** (the platform-level account, all 395 catalog permissions — functionally the same scope as the auto-created "Owner"), while **Admin is the company account**: the acting administrator for Luxor's own tenant, who is the one who actually builds out the company's teams (creates roles, sets their permissions, registers teammates, assigns them) rather than that being reserved for Super Admin. Admin therefore holds all 394 permissions except the one truly nuclear action (`core.company.deactivate` — deleting the tenant itself); it was initially scoped without `core.role.manage` and corrected once the intended hierarchy was clarified. Finance Team (78 perms: finance module + read-only cross-module) and Inventory Team (69 perms: inventory/warehouse + read-only cross-module) sit under Admin. See §6 below for the login/RBAC walkthrough, including proof that the **Admin account itself** — not Super Admin — created a new "Sales Team" role and registered/assigned a teammate to it end-to-end.

---

## 1. Correctness & Compliance Expert (GST / Finance)

**What works:** FX revaluation, GST e-invoicing (IRN/QR, sandboxed), e-way bill generation, and TDS/TCS withholding are all implemented with real GL double-entry posting (verified in code this session, not stubs).

**Issues found (live):**
- **No default Chart of Accounts on tenant creation.** A brand-new company (Luxor) starts with a completely empty GL — the Finance → Chart of Accounts screen shows "No accounts yet." Every account (Cash, AR, AP, Sales, COGS, etc.) must be created manually, one at a time, before a single invoice or journal entry can post. For an ERP pitched at Indian mid-market manufacturers, this is a hard blocker to a fast pilot — a business like Luxor with 8 factories and multi-entity accounting needs a starter chart of accounts on day one.
  - **Solution:** Ship an "Indian Manufacturing" starter chart-of-accounts template (Schedule III-aligned) that's seeded automatically on company creation, or offered as a one-click import during onboarding.
- **GST e-invoicing and e-way bill both rely on sandbox/fake GSP clients** (`SandboxGstIrpClient`, `SandboxEWayBillClient`) — there is no real GSTN/IRP integration yet. This is documented in code but not surfaced anywhere in the UI; a finance user generating an "e-invoice" today gets a syntactically valid but fake IRN with no legal standing.
  - **Solution:** Surface a visible "Sandbox mode — not connected to GSTN" banner on the e-invoice/e-way-bill screens until a real GSP credential is configured.
- **TDS/TCS is scoped to TDS-on-payments-to-suppliers only** — TCS on receivables (relevant for a company the size of Luxor selling above the TCS threshold) is explicitly not implemented (confirmed via handler doc comment).
  - **Solution:** Track as a near-term follow-up; the existing `WithholdingTaxRate`/`WithholdingTaxTransaction` domain generalizes to TCS with a `ReferenceType` of "Receivable" rather than a new domain.

---

## 2. Data Integrity Expert (Core / HRMS)

**Confirmed bug — Employee list silently drops the structured department link.**

Reproduced live: created 5 employees via the API, each with a real `DepartmentId` pointing at a genuine Core department (e.g., "Manufacturing", "Quality Assurance"). The HRMS → Employees screen's Department column shows **"—" for every employee**, even though they are correctly linked.

Root cause (confirmed in code): [`EmployeesPage.tsx:330`](../../frontend/src/modules/hrms/pages/EmployeesPage.tsx) renders `row.departmentName ?? '—'` — the free-text `DepartmentName` label field — and never resolves the structured `DepartmentId` foreign key into a display name. The create form has *both* a free-text "Department label" input and a "Department (Core)" searchable dropdown that sets `DepartmentId`; picking only the dropdown (the correct, structured way) leaves the list showing nothing.

- **Impact:** Any org that uses the proper structured department link (the intended path — Luxor has 8 real departments) will see every employee's department as blank in the list view, undermining a basic HR org-chart use case.
- **Solution:** Either (a) have the list column resolve `DepartmentId` → department name via the already-loaded `departmentOptions`, falling back to the free-text label only when no structured link exists, or (b) deprecate the free-text field entirely now that a real `Department` aggregate exists.

---

## 3. Platform Reliability Expert (API / Infra)

**Issue — rate limiter throttles legitimate bulk operations too aggressively.** Seeding 18 products via authenticated API calls in a tight loop hit `429 Too Many Requests` after the 10th call. This is a real, reproducible finding (not synthetic): a Luxor back-office user bulk-importing a product catalog via CSV/API (a common day-one workflow for a company with hundreds of SKUs) will be throttled well before finishing.
- **Solution:** Raise the burst limit for authenticated write endpoints, or expose a documented bulk/batch-create endpoint for masters (products, customers, suppliers) so legitimate imports don't need per-row throttling at all.

**Issue — Swagger/OpenAPI generation is broken.** `GET /swagger/v1/swagger.json` returns HTTP 500: `Can't use schemaId "$CreateAccountRequest"` — two different modules (`Finance.Api.Controllers.CreateAccountRequest` and `Crm.Api.Controllers.CreateAccountRequest`) collide because Swashbuckle keys schemas by short type name only. This has been true across the whole session — Swagger UI cannot render at all.
- **Impact:** No interactive API docs, no OpenAPI client generation, appears in every environment (not just this one).
- **Solution:** Configure Swashbuckle's `CustomSchemaIds` to use the full namespace-qualified name, or rename one of the colliding DTOs.

**Issue — `/health` requires authentication.** The health-check endpoint returns 401 for an anonymous request. Load balancers, container orchestrators, and uptime monitors conventionally expect `/health` to be reachable without credentials.
- **Solution:** Exempt `/health` (and any liveness/readiness probe route) from the auth middleware pipeline.

**Confirmed working:** Real EF Core migrations (this session baselined all 15 module schemas and swapped the dev-only bootstrap for genuine `Database.MigrateAsync()` calls); the app boots cleanly with zero "relation does not exist" errors against the reconciled schema.

**Environment note (not a product bug):** Kafka is unreachable in this environment (Docker Desktop isn't running locally), producing continuous `rdkafka` connection-retry log noise. The app degrades gracefully — outbox polling and REST APIs work regardless — but this confirms event-driven integrations (cross-module domain events) are silently non-functional without the message broker, with no visible health/readiness signal distinguishing "Kafka down" from "Kafka fine."
- **Solution:** Surface Kafka connectivity in a real `/health` check (once #4 above is fixed) so "degraded" is visible rather than silent.

---

## 4. Feature-Completeness Expert (cross-module)

Carried over from static analysis earlier this session and re-confirmed live in the Suppliers/Products/Customers screens (which correctly *do* have working Edit buttons — the gap is specifically the following 10):

| Module | Entity | Missing |
|---|---|---|
| BusinessIntelligence | KpiDefinition | Update endpoint |
| Crm | Campaign | Update endpoint |
| IntegrationHub | IntegrationConnector | Update endpoint |
| Maintenance | ChecklistTemplate | Update endpoint |
| Manufacturing | BillOfMaterials | Header Update (versioning exists, header edit doesn't) |
| Manufacturing | Machine | Update endpoint |
| Manufacturing | WorkCenter | Update endpoint |
| Marketplace | PluginListing | Update endpoint |
| Quality | InspectionPlan | Update endpoint |
| Sales | DiscountRule | Update endpoint |

Every other master-data entity in the platform (Suppliers, Customers, Products, Departments, Branches, etc.) has a working create → edit → deactivate lifecycle; these 10 are create → deactivate only, forcing a deactivate-and-recreate workaround for a simple typo fix.

---

## 6. RBAC / Access-Control Expert — role-based login walkthrough

Modeled Luxor's real reporting structure as 4 Core roles (Super Admin, Admin, Finance Team, Inventory Team) with distinct permission grants, registered one real user per role, and logged in as each to see what they actually see — not just what the permission catalog says they should have.

**Confirmed working — nav-level RBAC is correctly enforced.** Each role's sidebar showed exactly the modules its permission grant implies:
- **Inventory Team** (69 perms: inventory/warehouse full + read-only sales/procurement/manufacturing): sidebar showed only Core Platform, Inventory, Warehouse, Procurement, Sales, Manufacturing — Finance/CRM/HRMS/Quality/Maintenance/BI/AI/Marketplace/Integration Hub were correctly hidden.
- **Finance Team** (78 perms: finance full + read-only sales/procurement/HRMS-payroll/BI): sidebar showed only Core Platform, Procurement, Sales, Finance, HRMS, Business Intelligence.
- **Admin** (394 perms — all except `core.company.deactivate`): correctly saw all 15 modules.
- Backend enforcement matches: the Inventory Team dashboard's Finance/Procurement-scorecard widgets correctly returned `403 Forbidden` (confirmed via network inspection) rather than silently succeeding, and the underlying `IRequirePermission` checks held throughout.

**Confirmed working — the company account (Admin) genuinely builds its own team, not just Super Admin.** Per the intended hierarchy (Super Admin = software owner, Admin = the company account, which is the one that "makes teams and adds" people), Admin was granted `core.role.manage` and then, logged in as Admin (not Super Admin, not the original Owner), performed the full flow itself end-to-end via the real API: created a new **"Sales Team"** role, scoped it to 47 sales/CRM/read-only-adjacent permissions, registered a new teammate (`sales.lead@luxoroffice-demo.com`), and assigned them to the role it had just created. Verified visually afterward — the Admin login reaches `/core/roles` (previously blocked before the permission fix) and the created role appears in the list.

**Confirmed bug — page-level actions are not permission-gated, only nav-level routing is.** Logged in as Finance Team (which holds only `core.company.read`, not `update`/`deactivate`, and not `core.role.manage`/`core.feature-flag.manage`/`core.audit.read`): the Core Platform landing page still renders a fully live **"Edit"** and **"Deactivate"** button on the company record, plus clickable links to **Roles & permissions**, **Audit log**, **Settings**, and **Feature flags** — none of which this role can actually use. [`CompaniesPage.tsx`](../../frontend/src/modules/core/pages/CompaniesPage.tsx) never calls the app's own `useAuthStore.hasPermission()` helper (used correctly for nav-level gating in `RequirePermission.tsx`) before rendering these controls.
- **Impact:** confirmed backend-safe (the underlying commands require `core.company.update`/`core.company.deactivate`/`core.role.manage` and would reject a Finance Team caller with 403) — this is not a privilege-escalation hole. But it's a real defense-in-depth/UX gap: a Finance Team user sees "Deactivate" on the company they work for and has no way to know, without clicking it and hitting an error, that the button does nothing for them.
- **Solution:** wrap each action button/admin link in `CompaniesPage.tsx` (and audit sibling pages for the same pattern) with the same `hasPermission()` check already used for nav-level gating, matching the granularity the backend already enforces.

**Login flow itself:** works cleanly for every role — register → sign-in → JWT with the exact permission list embedded → sign-out all behaved correctly across all 4 accounts, no session bleed between users.

**Other gaps (not yet built, confirmed absent from the codebase this session):**
- No segregation-of-duties (SoD) checks — the same user can create and approve the same purchase order.
- BI dashboards are manual KPI snapshots, not live queries against operational data.
- No multi-entity/subsidiary consolidation — relevant to Luxor's real 8-factory, 5-regional-office structure, which in FusionOS today would need to be modeled as a single flat company rather than a true parent/subsidiary hierarchy.
- No billing/subscription domain — the platform itself has no way to meter or bill a customer like Luxor for usage.

---

## 7. Overall verdict

The platform genuinely runs, boots against real migrations, and correctly executes real business logic (GL double-entry, GST compliance windows, multi-tenant RBAC) rather than stubs — the Phase 0/Phase 1 work completed this session holds up under live use, across four differently-scoped real logins, not just the single bootstrap Owner account. The gaps found are concrete and actionable, not superficial: two real UI bugs (HRMS department display, unpermissioned action buttons on the Companies page), one real infra bug (Swagger schema collision), one operational risk (rate limiter vs. bulk import), and the previously-known feature gaps (10 missing Update endpoints, SoD, live BI, multi-entity, billing) are unchanged and still open.

**Priority order for fixes:**
1. Fix Swagger schemaId collision (trivial, unblocks all API tooling).
2. Fix HRMS employee department display (trivial, user-visible data-integrity bug).
3. Gate `CompaniesPage.tsx`'s action buttons/admin links behind `hasPermission()` (small, defense-in-depth — audit sibling pages for the same pattern).
4. Exempt `/health` from auth (trivial, unblocks ops tooling).
5. Raise/replace the bulk-write rate limit (small, blocks any real data migration/import).
6. Ship a starter chart of accounts (medium, blocks fast onboarding).
7. Continue the already-planned Phase 1.2–3 (installer/runbook, billing) and Phase 2 (10 Update endpoints, SoD, live BI, consolidation) work.

---

## Appendix A — Reproduction steps

1. `ASPNETCORE_ENVIRONMENT=Development dotnet run` from `backend/src/Host/FusionOS.Api.Host`.
2. `npm --prefix frontend run dev`, open `http://localhost:5173`.
3. Register a company via `/register` (any name/email works); sign in.
4. Seed masters via `POST /api/v1/{core/departments, inventory/products, sales/customers, procurement/suppliers, hrms/employees}` with a `CompanyId`/Bearer token from the signed-in session (see `docs/audit/luxor-2026/` scratch scripts for the exact payloads used).
5. Walk Inventory → Products, Sales → Customers, Procurement → Suppliers, HRMS → Employees, Finance → Chart of Accounts to reproduce each finding above.
6. For §6's RBAC walkthrough: 5 role accounts exist on this tenant, all with password `LuxorRole@2026!` — `superadmin@luxoroffice-demo.com` (Super Admin), `admin@luxoroffice-demo.com` (Admin — the company account, holds `core.role.manage`), `finance.lead@luxoroffice-demo.com` (Finance Team), `inventory.lead@luxoroffice-demo.com` (Inventory Team), and `sales.lead@luxoroffice-demo.com` (Sales Team — created by the Admin account itself, not Super Admin, to prove the intended hierarchy). Sign in as each to reproduce the nav-scoping and Companies-page findings. These are demo credentials on a local dev tenant only — rotate/delete before any shared or non-local use.
