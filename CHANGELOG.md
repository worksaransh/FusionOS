# Changelog

All notable changes to **FusionOS** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-07-27

### Added
- **15 Integrated ERP Modules**:
  - `Ai`: Predictive Demand Forecasting, Natural Language Search & Copilot, KPI Narrative Generator, Invoice OCR Processing.
  - `BusinessIntelligence`: Executive Dashboards, Custom KPI Builders, Scheduled Data Export Pipelines.
  - `Core`: User Management, RBAC Security, Multi-Tenant Architecture, Audit Logging, System Settings.
  - `Crm`: Lead Management, Sales Pipelines, Contact Tracking, Customer Portal.
  - `Finance`: Chart of Accounts, General Ledger, Accounts Receivable/Payable, GST Return Summaries, Financial Statements.
  - `Hrms`: Employee Directory, Attendance, Payroll, Leave Management, Onboarding/Offboarding.
  - `IntegrationHub`: Webhook Engine, REST API Connectors, ETL Data Migration Jobs.
  - `Inventory`: Product Master, Multi-Warehouse Stock Tracking, Serial Number & Batch Management.
  - `Maintenance`: Asset Management, Preventive Maintenance Checklists, Downtime Analytics.
  - `Manufacturing`: Bill of Materials (BOM), Work Orders, MRP Engine, Work Center Scheduling.
  - `Marketplace`: Plugin Marketplace, Third-Party Extension Management.
  - `Procurement`: Supplier Management, RFQs, Purchase Orders, Goods Receipt Processing.
  - `Quality`: Inspection Plans, Quality Audits, Deviations, Quality Holds & Certificates.
  - `Sales`: Quotations, Sales Orders, Customer Portals, Dispatch Management, Returns & Credit Notes.
  - `Warehouse`: Bin/Rack/Shelf Layout Management, Packages & Picking/Packing Operations.
- **Security & Architecture Foundation**:
  - Refactored .NET 8 Modular Monolith using CQRS and MediatR.
  - Tenant isolation pipeline behavior enforcing multi-tenant boundary checks on every request.
  - JWT Authentication with SHA-256 hashed refresh tokens.
  - Optimistic concurrency control via PostgreSQL `xmin` concurrency tokens.
- **Frontend Modernization**:
  - React 18 SPA built with Vite, TypeScript, and TailwindCSS.
  - Dynamic responsive layouts with dark mode, high-contrast metrics, and stats widgets.
  - Zustand auth store with automatic 401 token refresh retries.
- **DevOps & Observability**:
  - Complete Docker Compose stack with PostgreSQL, Redis, Kafka, OTel Collector, Prometheus, and Grafana.
  - Comprehensive GitHub Actions CI/CD pipelines.

### Changed
- Standardized documentation structure and API response schemas across all 15 modules.
- Refactored company creation flow to support zero-friction enterprise bootstrapping.
