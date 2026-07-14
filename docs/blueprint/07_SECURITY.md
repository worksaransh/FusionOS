# 07_SECURITY.md — FusionOS Security Standards

## 1. Authentication

- **JWT access tokens**, short-lived (~15 minutes), signed (asymmetric, RS256/ES256 — never HS256 with a shared secret sprawled across services).
- **Refresh tokens**, longer-lived, rotating (one-time use, replaced on every refresh; reuse of an already-rotated token is treated as a compromise signal and revokes the whole token family).
- **Two-Factor Authentication (2FA) ready**: TOTP-based 2FA is a Core Platform capability designed in at the identity-model level (per-user 2FA enrollment, backup codes) even if not enabled for every deployment tier at MVP.
- **Session management**: server-side session/refresh-token registry per device, so a user (or admin) can view and revoke active sessions — not purely stateless trust-the-JWT-until-expiry.
- **Future SSO**: OpenID Connect / SAML 2.0 federation against external IdPs (Azure AD/Entra, Okta, Keycloak) is designed for at the identity-provider abstraction layer now, even though direct username/password plus 2FA is the MVP default.

## 2. Authorization

- **RBAC** as the baseline model: Roles composed of Permissions, assigned per user per Company (supporting the multi-company scenario — a user's role in Company A is independent of their role in Company B).
- **Fine-grained permission claims** beyond coarse roles where the domain demands it (e.g., "approve purchase orders up to ₹500,000" as a scoped permission, not just "Procurement Approver").
- Authorization is enforced **twice**: at the API layer (every command/query handler checks permissions via a MediatR pipeline behavior — no controller action trusts the frontend) and reflected into the UI (`06_UI_UX_DESIGN_SYSTEM.md` §8) for usability. The API check is the actual security boundary; the UI check is UX only.
- **Row-level data scoping** (CompanyId/BranchId per `04_DATABASE_GUIDELINES.md`) is enforced as a global query filter, not a per-query manual `WHERE` clause developers might forget.

## 3. Audit Logging

Every authentication event (login, logout, failed login, token refresh, 2FA challenge), every permission change, and every business-data mutation is audit-logged per `04_DATABASE_GUIDELINES.md` §5. Audit logs are immutable and queryable by security/compliance roles through a dedicated, permission-gated view — never exposed generally.

## 4. Encryption

- **At rest**: database-level encryption (cloud provider KMS-backed volume encryption at minimum; column-level encryption for especially sensitive fields — bank account numbers, tax IDs, payment credentials — using envelope encryption with keys in a KMS/Key Vault, never hardcoded or config-file keys).
- **In transit**: TLS 1.2+ everywhere, including internal service-to-service and database connections — no plaintext internal traffic on the assumption that "it's inside the cluster."
- **Secrets management**: application secrets (DB credentials, API keys for integrations, signing keys) live in a secrets manager (cloud KMS/Key Vault for SaaS, a local encrypted vault such as HashiCorp Vault for on-prem) — never in source control, environment files committed to repos, or plaintext config.

## 5. Password Storage

Passwords hashed with **Argon2id** (preferred) or bcrypt with a modern work factor — never MD5/SHA-family unsalted or reversible encryption. Password policy (minimum length/complexity, breach-list checking against a service like Have I Been Pwned's k-anonymity API) enforced at registration/change, not just documented.

## 6. Input Validation & Injection Defense

FluentValidation (backend) and Zod (frontend) on every input per `02_TECH_STACK.md`; all database access through EF Core parameterized queries — no string-concatenated SQL anywhere in the codebase; file uploads (File Management, OCR/invoice processing in AI Platform) are type/size validated and scanned before storage.

## 7. API Security

Rate limiting per `08_API_STANDARDS.md` doubles as an abuse/brute-force control on authentication endpoints specifically (stricter limits on `/auth/login`, `/auth/refresh` than general API traffic). CORS configured per deployment, never `*` in production. Standard security headers (CSP, HSTS, X-Content-Type-Options, etc.) applied platform-wide via middleware, not per-controller.

## 8. Multi-Tenancy Isolation

Because Cloud deployments run many companies in shared infrastructure, tenant isolation is treated as a security boundary, not just a data-filtering convenience: global query filters (application layer) plus PostgreSQL Row-Level Security (database layer) as defense-in-depth, so a bug in one layer alone cannot leak cross-company data. Penetration testing of tenant-isolation specifically is a required pre-release check, not an optional nice-to-have.

## 9. Third-Party & Integration Security

IntegrationHub connector credentials (Shopify, Amazon, Razorpay, Stripe, WhatsApp, etc. — `03_SYSTEM_ARCHITECTURE.md` §7) are company-scoped, encrypted, and least-privilege scoped to only the external API permissions each connector actually needs. Webhook endpoints verify signatures from the external provider before processing (never trust an unauthenticated inbound webhook payload).

## 10. Backup, HA & Disaster Recovery

Coordinated with `04_DATABASE_GUIDELINES.md` §11: encrypted backups, tested restore procedures, and documented RTO/RPO per deployment tier. Disaster recovery is treated as a security control (protecting availability and integrity), reviewed on the same cadence as access-control reviews.

## 11. Security Review as a Release Gate

Per `01_PROJECT_RULES.md`, no module ships without an explicit security review covering: authentication/authorization correctness, input validation coverage, audit log completeness, encryption of sensitive fields, and tenant-isolation correctness for any new table or endpoint. This review is recorded (not verbal), so it can be revisited when the module is later extended.

## 12. Vulnerability & Dependency Management

Automated dependency scanning (Dependabot/Snyk equivalent) in CI; static application security testing (SAST) integrated into the pipeline described in `02_TECH_STACK.md` §9; a documented responsible-disclosure process once FusionOS has external users, so security reports have a clear intake path rather than reaching engineers ad hoc.
