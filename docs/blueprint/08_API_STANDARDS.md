# 08_API_STANDARDS.md — FusionOS API Standards

## 1. Style & Documentation

REST over HTTPS. Every endpoint documented via OpenAPI 3.1, generated from code (Swashbuckle per `02_TECH_STACK.md`) — the spec is never hand-maintained separately from the implementation, and CI fails if the generated spec and committed reference spec diverge.

## 2. Versioning

All endpoints are versioned in the URL path: `/api/v1/{module}/{resource}`. A breaking change to a resource requires a new version segment (`/api/v2/...`); the prior version remains available through a documented deprecation window (minimum two release cycles), announced in the changelog before removal. Additive changes (new optional fields, new endpoints) do not require a version bump.

## 3. Resource & URL Conventions

- Resource names are plural nouns: `/api/v1/inventory/products`, `/api/v1/procurement/purchase-orders`.
- Nested resources reflect real ownership, not arbitrary convenience: `/api/v1/manufacturing/production-orders/{id}/material-issues`.
- Actions that are not pure CRUD are modeled as sub-resources or explicit verbs on a resource, not overloaded HTTP verbs: `POST /api/v1/procurement/purchase-orders/{id}/approve`, not a generic `PATCH` with a magic status field.

## 4. Pagination, Filtering, Sorting, Search

Mandatory on every list endpoint — no module ships a list endpoint that returns unbounded results:

- **Pagination**: cursor-based for high-volume tables (Inventory Ledger, Audit Logs) to stay performant at 10M+ rows; offset/limit acceptable for smaller, bounded resource sets. Response includes `pageInfo` (next/previous cursor or page, total count where cheaply computable).
- **Filtering**: standardized query parameter syntax (`?filter[status]=open&filter[companyId]=...`), with an allow-listed, documented set of filterable fields per resource — not arbitrary raw SQL passthrough.
- **Sorting**: `?sort=-createdAt,name` convention (prefix `-` for descending), allow-listed sortable fields.
- **Search**: `?q=` for free-text search where the resource supports it, backed by the search infrastructure in `02_TECH_STACK.md` §3, meeting the sub-second search performance target.

## 5. Bulk Operations

Supported where the domain justifies volume (stock adjustments, price list updates, bulk PO approval): a dedicated bulk endpoint (`POST /api/v1/inventory/stock-adjustments/bulk`) accepting an array payload, returning a per-item result set (partial success is explicit and itemized — a bulk call never silently succeeds for some items and fails for others without reporting which).

## 6. Validation & Error Format

- Input validation via FluentValidation (backend) executes before any domain logic runs; validation failures return `400 Bad Request` with a standardized error body:

```json
{
  "type": "https://fusionos.dev/errors/validation-failed",
  "title": "Validation failed",
  "status": 400,
  "traceId": "...",
  "errors": {
    "sku": ["SKU is required.", "SKU must be unique within the company."]
  }
}
```

- This follows **RFC 7807 (Problem Details)** as the shared error envelope for all non-2xx responses across every module — a consumer of the API learns the error shape once, not per module.
- Optimistic concurrency conflicts (`RowVersion` mismatch, `04_DATABASE_GUIDELINES.md` §8) return `409 Conflict` with the current server-side version of the resource included, so clients can offer a merge/retry UX.

## 7. Rate Limiting

Applied platform-wide via middleware, tiered: stricter limits on authentication endpoints (see `07_SECURITY.md` §7), generous but bounded limits on standard CRUD/list endpoints, and separate quota tracking for IntegrationHub-facing webhook endpoints. Rate-limited responses return `429 Too Many Requests` with a `Retry-After` header — never a silent drop.

## 8. Authentication on Every Endpoint

Every endpoint requires a valid JWT unless explicitly and deliberately marked anonymous (e.g., public webhook receivers, which instead verify provider signatures per `07_SECURITY.md` §9). There is no "internal-only, no auth needed" endpoint inside the monolith — module boundaries are respected at the API layer even for module-to-module traffic that happens to run in the same process today.

## 9. Idempotency

State-changing endpoints that may be retried by clients (mobile apps with intermittent connectivity, integration connectors) support an `Idempotency-Key` header; the API layer deduplicates on this key so a retried `POST` (e.g., "create sales order") cannot double-create a record.

## 10. Webhooks (Inbound & Outbound)

Treated as first-class API surface, not an afterthought bolted onto IntegrationHub:

- **Outbound webhooks** FusionOS sends to customer-configured endpoints (e.g., "SalesOrderConfirmed") are versioned identically to REST resources, signed (HMAC) so receivers can verify authenticity, and retried with exponential backoff on failure, with delivery status visible to the customer.
- **Inbound webhooks** (from Shopify, Razorpay, Stripe, WhatsApp, etc.) are received by IntegrationHub, signature-verified per `07_SECURITY.md` §9, and translated into internal commands — never processed as trusted input directly against domain services.

## 11. Response Shape Consistency

Single-resource responses return the resource directly (not double-wrapped); list responses return `{ "data": [...], "pageInfo": {...} }`. Field naming is `camelCase` in JSON regardless of backend `PascalCase` convention (`09_CODING_STANDARDS.md`), consistent across every module's API without exception.

## 12. API Review Gate

Per `01_PROJECT_RULES.md`, no endpoint ships without: OpenAPI documentation, validation coverage, authorization checks, rate-limit classification, and a decision on pagination/filtering/sorting support (even if the decision is "not applicable, resource is a singleton") recorded at review time.
