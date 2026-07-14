# 12_AI_PLATFORM.md — AI Platform Architecture

## 1. Principle: AI Is Infrastructure, Not a Chatbot

AI in FusionOS is a platform feature woven through every module's data and event stream, not a standalone assistant window bolted on top. Every capability below consumes the same integration-event backbone described in `03_SYSTEM_ARCHITECTURE.md` §4 — AI features react to real business events in near-real-time, not a nightly data-warehouse batch job.

## 2. Required AI Capabilities (per PRD)

FusionOS AI Copilot · Demand forecasting · Inventory optimization · Purchase recommendation · Production planning assistance · OCR/invoice processing · Natural-language search · Automated report generation · Predictive analytics · Root-cause analysis. These are treated as platform services other modules call and subscribe to, not features hidden inside a single "AI module" UI.

## 3. Architectural Placement

The **AI module** (per `05_MODULE_ROADMAP.md`) is architecturally a consumer of every other module's integration events and a provider of recommendation/insight events back into the platform — it never sits in the synchronous critical path of a core transaction (a sale, a goods receipt, a production order confirmation must never block on an AI service being available or fast). This is enforced the same way BI is isolated (`05_MODULE_ROADMAP.md` §2): AI outages degrade insight quality, never transactional throughput.

### 3.1 Hybrid Runtime (deliberate exception to the .NET-first rule)

`02_TECH_STACK.md` establishes .NET as the primary backend, with Python explicitly scoped to AI/ML given the maturity of the Python ML ecosystem (PyTorch, scikit-learn, pandas, Hugging Face). The AI module is implemented as:

- **Python ML services** (forecasting models, OCR pipelines, embedding generation) — stateless, containerized identically to every other module per `02_TECH_STACK.md` §9, communicating with the rest of the platform *only* through the integration-event bus and a versioned internal API — never via direct database access into other modules' schemas, and never via other modules reaching into AI's schema either.
- **.NET AI orchestration layer** inside the monolith — receives AI-produced recommendations/insights as events, applies business rules (e.g., "only surface a reorder recommendation if it doesn't violate a budget hold"), and exposes them through the standard `/api/v1/ai/...` REST surface per `08_API_STANDARDS.md`, so frontend/mobile clients have one consistent API pattern regardless of what runs underneath.

## 4. Capability-by-Capability Design

**Demand forecasting** — time-series models trained on the Inventory Ledger and Sales history (`InventoryAdjusted.v1`, `SalesOrderConfirmed.v1` events accumulated over time), retrained on a scheduled cadence (Scheduler, Core Platform) plus incrementally as new transaction volume arrives; outputs a versioned forecast per SKU/warehouse/company, consumed by Inventory (reorder points) and Manufacturing (MRP inputs).

**Inventory optimization & purchase recommendation** — combines forecast output, current stock levels, lead times (from Procurement's supplier/price history), and cost data (Finance) to recommend reorder quantities/timing; surfaced as an actionable recommendation in the UI (`06_UI_UX_DESIGN_SYSTEM.md`), never auto-executed without a human approval step at MVP — auto-execution is a later, explicitly opt-in capability once trust in recommendation quality is established per customer.

**Production planning** — consumes Manufacturing's BOM/routing/capacity data plus demand forecasts to suggest production schedules; interacts with MRP as a recommendation layer, not a replacement for MRP's deterministic calculation.

**OCR / invoice processing** — document ingestion (via Core's File Management) → OCR/extraction (Python service, likely leveraging a vision-capable model) → structured data proposed back into Procurement (vendor invoice matching) or Finance (AP entry) as a draft record requiring human confirmation before posting — OCR output is a draft, never auto-posted to the ledger.

**Natural-language search** — a platform-wide capability (extends Core's Search) letting users query across modules in natural language ("show me overdue purchase orders from suppliers in Gujarat"), translated into structured queries against each module's published read APIs — this is a translation layer over existing authorized queries, not a bypass of RBAC/permission checks (`07_SECURITY.md`); a user can never retrieve via natural-language search what they could not retrieve via the normal API.

**Automated report generation** — building on Business Intelligence (`05_MODULE_ROADMAP.md` Phase 6), generates narrative summaries and formatted reports from the same dashboards/KPIs BI already computes, rather than maintaining a parallel reporting pipeline.

**Predictive analytics & root-cause analysis** — cross-module pattern detection (e.g., "late deliveries correlate with a specific supplier and a specific warehouse's receiving backlog") consuming the full integration-event history; surfaced as investigative insights in BI dashboards and the AI Copilot, always with the underlying data/events cited so a human can verify the reasoning, not just trust a black-box conclusion.

**AI Copilot** — the conversational surface tying the above together; architecturally a client of the same `/api/v1/ai/...` and module APIs any other client uses, plus a retrieval layer (pgvector/Qdrant per `02_TECH_STACK.md` §3) over documentation, historical tickets, and permitted business data scoped to the asking user's RBAC permissions — the Copilot can never answer with data the user isn't independently authorized to see.

## 5. Data & Model Governance

- All AI-consumed data (customer records, financials, employee data) respects the same tenant isolation as the rest of the platform (`07_SECURITY.md` §8) — no cross-company data leakage through shared model training; models are trained per-company or with company data properly isolated/anonymized if any shared-model approach is ever used.
- Model versioning: every forecast/recommendation is tagged with the model version that produced it, so quality regressions are traceable and A/B comparisons are possible.
- Human-in-the-loop by default: recommendation-class outputs (reorder suggestions, production schedule suggestions, OCR-extracted invoices) require explicit user confirmation before they affect the transactional ledger, at least through the initial roadmap phases (`05_MODULE_ROADMAP.md` Phase 7); full automation is an explicit, separately reviewed opt-in per capability once accuracy is proven.
- Explainability: wherever practical, recommendations surface the contributing factors (e.g., "recommended reorder qty based on 90-day demand trend + current lead time"), not just a number — this is a product requirement, not a nice-to-have, because ERP users need to trust and audit AI-driven decisions the same way they audit any financial figure.

## 6. Performance & Reliability

AI services meet the same observability standard as any module (`09_CODING_STANDARDS.md` §6) but are explicitly allowed a looser latency budget than the < 300ms API target (`09_CODING_STANDARDS.md` §7) for genuinely heavy operations (model inference, OCR) — these are asynchronous (job submitted, result delivered via notification/event) rather than forced into a synchronous request-response pattern that would otherwise violate platform-wide latency targets.

## 7. Future Extensibility

Third-party AI agents are a named Marketplace category (`05_MODULE_ROADMAP.md` Phase 8) — the same event-subscription and scoped-permission model used for plugins generally (`03_SYSTEM_ARCHITECTURE.md` §6) applies to AI agents, so a customer or partner can add a specialized AI capability (e.g., an industry-specific quality-defect-detection model) without FusionOS's core AI module needing to natively support every conceivable use case.
