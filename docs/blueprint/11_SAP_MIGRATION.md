# 11_SAP_MIGRATION.md — Legacy System Migration Strategy

Note on scope: the PRD requires migration support from more than SAP alone — SAP Business One, SAP S/4HANA, Oracle, Microsoft Dynamics, Odoo, ERPNext, Tally, Busy, Zoho, Excel/CSV, and generic SQL databases. SAP is named in this document's title because it is the hardest and highest-value case (largest enterprise customers, most complex data model); the strategy below is designed to generalize to every source system, with SAP as the reference implementation.

## 1. Why This Is Architecture, Not a Feature

Migration tooling is treated as a Core Platform capability (`05_MODULE_ROADMAP.md` — Phase 10), not a one-off import script per customer. Every source-system connector implements the same internal contract, so adding a new source system (e.g., a regional ERP not yet listed) is additive work, not a new subsystem.

## 2. Migration Architecture

```
Source System → Extractor → Staging Schema → Mapping/Transform → Validation → FusionOS Load → Reconciliation
```

- **Extractor**: source-specific adapter (SAP: OData/RFC or supported export files; Oracle/Dynamics: their respective APIs/export formats; Odoo/ERPNext: their REST APIs; Tally/Busy: their XML/export formats; Zoho: REST API; Excel/CSV/SQL: direct file/connection ingestion). Extractors are read-only against the source — FusionOS never writes back to a source system during migration.
- **Staging schema**: a dedicated `migration.*` schema in PostgreSQL (per `04_DATABASE_GUIDELINES.md` conventions) holding raw extracted data as-is, before any transformation — preserves an audit trail of exactly what was read from the source.
- **Mapping/Transform**: declarative field-mapping configuration per source system and per FusionOS entity (e.g., SAP `MARA`/`MARC` material master → FusionOS `inventory.products`/`inventory.product_warehouse_settings`), versioned and reviewable, not hardcoded transform logic buried in a script.
- **Validation**: business-rule validation (the same FluentValidation rules real transactions go through — `02_TECH_STACK.md`) runs against transformed records before load; failures are reported per-record with the reason, never silently dropped or silently coerced.
- **Load**: performed through the same Application-layer commands/APIs real users would use (not direct SQL inserts bypassing domain invariants), ensuring migrated data is exactly as valid as natively entered data — audit trail records the migration job as the `CreatedBy` actor.
- **Reconciliation**: automated post-load comparison (record counts, control totals — e.g., total inventory value, open AR/AP balances) between source and FusionOS, signed off before cutover.

## 3. Migration Phasing per Customer Engagement

1. **Discovery** — inventory of source system(s) in use, data volumes, customizations, and which FusionOS modules are in scope for go-live.
2. **Mapping workshop** — confirm field mappings and any data-cleansing rules (e.g., deduplicating customer records, standardizing UOM codes) with the customer's business owners, not just IT.
3. **Trial migration** — run the full pipeline into a non-production FusionOS environment; customer validates a statistically meaningful sample plus all control totals.
4. **Parallel run** (for complex/high-risk migrations, especially SAP S/4HANA replacements) — source system and FusionOS run side by side for an agreed period, with reconciliation reports comparing outputs (e.g., matching invoices, matching stock positions) before the source system is retired.
5. **Cutover** — final delta migration (changes since trial migration), freeze on the source system, go-live on FusionOS, source system moved to read-only archive access.
6. **Post-go-live support window** — elevated monitoring and a fast-response path for data discrepancies discovered in the first operating cycles (e.g., first month-end close on FusionOS).

## 4. Source-System-Specific Notes

- **SAP Business One / S/4HANA**: highest complexity — chart of accounts structure, cost center hierarchies, and BOM/routing data typically require the most mapping-workshop time. S/4HANA access is via supported exports/APIs only (no direct database access assumed, since S/4HANA customers rarely grant it and SAP's licensing/support terms generally prohibit it).
- **Oracle / Microsoft Dynamics**: similar complexity profile to SAP; connectors reuse the same staging/validation/reconciliation pipeline with source-specific extractors.
- **Odoo / ERPNext**: lower complexity — both expose reasonably complete REST APIs and comparable open data models, making field mapping more direct.
- **Tally / Busy**: common in the Indian SMB/trading segment (a named target customer base); primarily financial/inventory data via export files (XML for Tally) — high volume of straightforward migrations expected here, so this path is prioritized for tooling polish and self-service (customer-run) migration, not only consultant-assisted.
- **Zoho**: REST API-based extraction, moderate complexity.
- **Excel/CSV/SQL**: the generic fallback path — a configurable, self-service import tool (template-based column mapping, validation preview before commit) available to every customer regardless of source system, since almost every migration has some long-tail data that only exists in a spreadsheet.

## 5. Data Integrity Guarantees During Migration

- No hard deletes, ever — consistent with `04_DATABASE_GUIDELINES.md` §4, migrated records that are later found to be erroneous are corrected via reversal/correction entries, not deletion, preserving the audit trail from day one of the migrated history.
- Full audit trail: every migrated record's audit columns (`CreatedBy`, etc.) identify the migration job and source system, so migrated data is always distinguishable from natively entered data if ever needed for investigation.
- Historical transactions (not just current balances) are migrated where the source system and customer timeline justify it, specifically to support FusionOS's Business Intelligence and AI forecasting modules having meaningful historical data from go-live rather than a cold start.

## 6. Success Criteria for a Migration

Reconciliation totals match within an agreed tolerance (target: exact match for financial control totals); the customer's business users can complete their standard operating cycle (e.g., a sales-order-to-invoice flow, a purchase-to-payment flow) entirely in FusionOS before the source system is retired; and no data loss occurred for any record class in scope, verified by the staging-schema audit trail in §2.
