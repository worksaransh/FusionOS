# ISO/IEC 27001:2022 Gap Analysis — FusionOS

**Scope:** the FusionOS codebase and its deployment artifacts (Docker Compose stack, Windows installer, CI/CD workflows) as they exist today.

**Method:** every finding below comes from reading actual source code and config files, cited by path and line. Nothing here is drawn from `docs/blueprint/07_SECURITY.md`, which describes an *aspirational* security design — several of its claims (Argon2id hashing, RS256/ES256 JWTs, column-level encryption, RLS, SSO) are **not** implemented today. Where this document and the blueprint disagree, this document reflects what actually runs.

**Status legend:**
- **Implemented** — real, working code enforces the control.
- **Partial** — some real enforcement exists, with a specific named gap.
- **Absent** — searched, found nothing.
- **Organizational** — an ISO 27001 Annex A control that governs people/process/facilities, not application code. No source-code evidence is possible or expected; these are tracked as management-system action items, not engineering tickets.

Annex A of ISO/IEC 27001:2022 has 93 controls across 4 themes (A.5 Organizational — 37, A.6 People — 8, A.7 Physical — 14, A.8 Technological — 34). This document maps FusionOS's current state against the controls a software engineering audit can actually speak to, and flags the rest as organizational.

---

## Executive summary

| Theme | Controls with code evidence | Implemented | Partial | Absent |
|---|---|---|---|---|
| A.5 Organizational | 3 (policy, supplier security, incident mgmt process hooks) | 0 | 1 | 2 |
| A.6 People | 0 (all organizational — screening, training, disciplinary process) | — | — | — |
| A.7 Physical | 0 (all organizational/hosting-provider — office/datacenter security) | — | — | — |
| A.8 Technological | 20 | 8 | 8 | 4 |

**Headline gaps, in order of severity:**

1. **No field-level encryption for sensitive data**, and no TLS on the database connection in any checked-in config (A.8.24).
2. **No secrets manager/KMS anywhere** — DB passwords and JWT signing keys live in plaintext `appsettings.json`/environment variables/the Windows installer's generated config file (A.8.24, A.5.17).
3. **No second authentication factor** — a compromised password is a full account compromise, no TOTP/2FA step exists in the login flow (A.8.5, A.5.17).
4. **Tenant isolation is enforced only in the application layer** (a MediatR pipeline behavior), never at the database engine — no PostgreSQL Row-Level Security exists as defense-in-depth (A.8.3, A.8.24).
5. **Backups are unencrypted and unscheduled** — `pg_dump` output has no encryption step, and nothing in the repo or either Compose file actually triggers a backup on a cadence; the documented RPO is aspirational, not real (A.8.13).
6. **No real SAST tool** — `security-scan.yml`'s dependency checks are explicitly non-blocking (`|| true`), so a build never fails on a known vulnerability (A.8.29).
7. **No SSO/OIDC/SAML** — every login is username+password against FusionOS's own user store; there's no federation option for enterprise customers who require it (A.8.5, A.5.16).

None of this blocks FusionOS from *starting* an ISO 27001 readiness program — an ISMS is a management system, and Annex A controls 1, 2, and half of the organizational/people/physical set are process work independent of the codebase (see `REMEDIATION_ROADMAP.md` for what's genuinely blocking vs. what's paperwork). But items 1–4 above are the ones that would fail a Stage 1 or Stage 2 external audit outright if a certification date were set today.

---

## A.8 — Technological controls

### A.8.2 Privileged access rights
**Partial.** RBAC (`AuthorizationBehavior.cs`) enforces per-permission checks on every MediatR request that isn't explicitly public, failing closed on anything unclassified — `backend/src/Shared/FusionOS.BuildingBlocks.Application/Behaviors/AuthorizationBehavior.cs:46-60`. But there is no separate "privileged account" concept (break-glass admin, just-in-time elevation, or periodic recertification of who holds `core.role.manage`/Owner-role membership) — a company's Owner role, once granted, holds every permission indefinitely with no re-review mechanism. This session's own live testing found Owner roles going *stale* relative to the permission catalog (see `docs/compliance/iso27001/README.md` for the incident) — the opposite direction of risk from over-privilege, but evidence that role/permission state isn't actively reconciled.

### A.8.3 Information access restriction
**Partial.** Enforced at the application layer via `TenantIsolationBehavior.cs` (`backend/src/Shared/FusionOS.BuildingBlocks.Application/Behaviors/TenantIsolationBehavior.cs:39-46`) — reflectively compares a request's `CompanyId` to the caller's JWT claim, throwing `ForbiddenException` on mismatch. Gap: this is the *only* layer. `BaseDbContext.cs:37-47` applies a global EF Core query filter for `IsDeleted == false` only — there is no `CompanyId`-scoped filter, and no PostgreSQL Row-Level Security policy anywhere in the schema or migrations (repo-wide grep for `CREATE POLICY`/`ENABLE ROW LEVEL SECURITY` returns zero hits). A bug in a single pipeline behavior — or a raw SQL query that bypasses MediatR entirely — is the only thing standing between one company's data and another's. See A.8.24 for the related database-encryption gap.

### A.8.4 Access to source code
**Organizational.** Governed by the Git hosting platform's own access controls (branch protection, repo permissions) — not something this codebase's own source can attest to. Track under the ISMS's access-control policy, not as an engineering gap.

### A.8.5 Secure authentication
**Partial.** JWT-based authentication is real: `JwtTokenService.cs` issues HS256-signed access tokens (`backend/src/Modules/Core/FusionOS.Modules.Core.Infrastructure/Security/JwtTokenService.cs:42-44`), 15-minute lifetime, with rotating opaque refresh tokens (64 random bytes, only a SHA-256 hash persisted — `JwtTokenService.cs:56-66`; reuse of an already-rotated token fails lookup, `RefreshTokenCommandHandler.cs:53-59`). Passwords hashed with BCrypt, work factor 11 (`BCryptPasswordHasher.cs:8`). **Gap: no second factor.** `LoginCommandHandler.cs:31-33` goes straight from password verification to token issuance. A repo-wide search for `totp|2fa|mfa|authenticator` finds zero implementation — only mentions in roadmap docs. **Gap: symmetric JWT signing.** `HmacSha256` with a shared secret (`Jwt:SigningKey`) means every process that can validate a token can also mint one; the blueprint's own aspiration (RS256/ES256 asymmetric signing) was never built. **Gap: no SSO/OIDC/SAML** — see A.8.5 cross-reference below and the dedicated entry under People/Organizational controls for enterprise-federation policy implications.

### A.8.6 Capacity management
**Absent (as a formal control).** No code evidence of capacity monitoring/alerting thresholds tied to an incident-response trigger. `docker-compose.yml`'s observability stack (Prometheus/Grafana/Loki, referenced in prior session work) provides raw metrics, but nothing enforces a capacity *policy* (e.g., alert-and-scale before disk/connection-pool exhaustion). Track as a remediation item, not a fabricated "implemented."

### A.8.7 Protection against malware
**Absent.** No antivirus/malware-scanning step found for file uploads (Document Management, OCR/invoice ingestion). `UploadDocumentValidator.cs` (referenced in prior session summaries) validates file size/type only — no content scanning. This is a real gap for any customer-facing file upload surface.

### A.8.8 Management of technical vulnerabilities
**Partial.** Dependabot is real and configured across 4 ecosystems (`nuget`, `npm`, `docker`, `github-actions`) with weekly scheduled PRs — `.github/dependabot.yml`. But `.github/workflows/security-scan.yml` runs `npm audit --audit-level=high || true` (line 25) and `dotnet list package --vulnerable --include-transitive || true` (line 34) — the `|| true` means **a known-vulnerable dependency never fails CI**. No SAST tool (CodeQL, Semgrep, Snyk) is configured anywhere.

### A.8.9 Configuration management
**Partial.** Infrastructure-as-config exists (`docker-compose.yml`, `appsettings.*.json` per environment, the Inno Setup installer script) but there's no drift-detection or configuration-baseline enforcement — an operator hand-editing `appsettings.Production.json` on an on-prem box (which the installer's own UI text explicitly invites, per A.8.24 below) has no guardrail against a misconfiguration reintroducing a fixed vulnerability.

### A.8.10 Information deletion
**Partial.** Every aggregate root supports soft-delete (`IsDeleted`) with the corresponding EF Core global query filter (`BaseDbContext.cs:37-47`) — deleted rows are excluded from queries but physically retained indefinitely. There is no hard-delete/purge mechanism for data-retention-policy compliance (e.g., "erase this company's data after contract termination + N years") — a real gap if FusionOS operates in a jurisdiction with a right-to-erasure requirement (GDPR Art. 17 or equivalent).

### A.8.11 Data masking
**Absent.** No masking/redaction found for sensitive fields in logs, audit trails, or non-production environments — `AuditBehavior.cs`'s `ChangesJson` field is hard-coded `null` today (see A.8.15 below) so this is currently moot for audit logs specifically, but there's no masking strategy for e.g. seeding a staging environment from a production snapshot.

### A.8.12 Data leakage prevention
**Absent.** No DLP tooling or egress-monitoring control found. Out of scope for a single-codebase audit in most cases, but worth naming explicitly rather than silently omitting.

### A.8.13 Information backup
**Partial.** `scripts/backup-postgres.sh` is real (`pg_dump --format=custom --compress=9`, lines 35-42) with local retention (`KEEP_BACKUPS`, default 14, lines 46-53) and a matching, interactively-confirmed `scripts/restore-postgres.sh`. Gaps, all explicit in the scripts' own comments: **no encryption of the backup output** (no `gpg`/`openssl enc` step), **no offsite replication** ("shipping it offsite... is a follow-up once one exists" — `backup-postgres.sh:15-19`), and **no scheduler** — nothing in the repo or either Compose file actually *runs* the backup script on a cadence. `docs/deployment/ONPREM_RUNBOOK.md:238-256` documents a measured RTO (~3.66s, on a trivial dev dataset, explicitly caveated as unrepresentative at production scale) but states plainly that RPO is **not real** — "no real RPO — only whatever interval a human remembers to run it at manually."

### A.8.14 Redundancy of information processing facilities
**Organizational/infra.** Governed by deployment topology (cloud SaaS vs. on-prem installer) rather than application code. The on-prem Windows installer is explicitly single-machine, single-process (`docs/deployment/ONPREM_RUNBOOK.md` §0) — no redundancy by design for that deployment tier; the Docker Compose stack likewise runs single instances of Postgres/Redis/Kafka with no failover configuration. Track as a deployment-tier decision, not a code defect.

### A.8.15 Logging
**Partial.** `AuditBehavior.cs` (`backend/src/Shared/FusionOS.BuildingBlocks.Application/Behaviors/AuditBehavior.cs:21-51`) captures `EntityType`, `EntityId`, `Action`, `ActorId`, `CompanyId`, `BranchId`, `OccurredAt`, `CorrelationId` for every `IAuditableCommand` — broad adoption (referenced across 1,072 files under `backend/src`). Three concrete gaps: **(1)** `ChangesJson` is hard-coded to `null` (line 46) — no before/after field diff is ever actually captured, despite the schema having a column reserved for it. **(2)** Only commands are audited; every `IQuery` (read) and every pre-auth action (login, failed login, register) produces zero audit trail — an ISO 27001 auditor will specifically ask for evidence of failed-login monitoring, which does not exist. **(3)** No centralized, tamper-evident log shipping — audit rows live in the same application database they describe, with no write-once storage or external SIEM forwarding.

### A.8.16 Monitoring activities
**Partial.** Observability infrastructure (Prometheus/Grafana/Loki, OpenTelemetry) exists per prior session work, but no code evidence of *security-specific* alerting rules (e.g., alert on N failed logins from one IP, alert on a permission-denied spike). Rate limiting (A.8.5-adjacent, see below) throttles but does not alert.

### A.8.17 Clock synchronization
**Organizational/infra.** No application-level NTP configuration to audit; this is a deployment/OS concern. Not a code gap.

### A.8.18 Use of privileged utility programs
**Organizational.** No evidence of unrestricted privileged tooling in the application itself; standard OS/infra hardening concern, out of scope for this codebase.

### A.8.19 Installation of software on operational systems
**Partial.** The Windows installer (`installer/FusionOS.iss`) requires `PrivilegesRequired=admin` for install, which is appropriate, but performs no code-signing (the installer itself is unsigned, and the resulting `.exe` triggers an unsigned-publisher warning) — no supply-chain attestation that the installed binary is what FusionOS actually built.

### A.8.20 Networks security
**Partial.** CORS is allow-list only (`Program.cs:181-189`, origins from `Cors:AllowedOrigins` config, empty by default) with `AllowCredentials()` set when any origin is configured — standard, but credentialed CORS widens the blast radius of any origin-list misconfiguration. No network segmentation control is visible from the codebase (that's an infra/deployment concern). **No TLS/SSL parameter on any database connection string checked into the repo** (`docker-compose.yml:104`, `appsettings.Development.json`) — see A.8.24.

### A.8.21 Security of network services
**Partial.** Rate limiting is real: `AspNetCoreRateLimit` (`Program.cs:1, 175-178, 383`), global limits (`10 req/s`, `300 req/15min`) plus a deliberately stricter `5 req/min` on `POST /api/v1/core/auth/login` specifically — confirmed live during this session's own testing (the exercise script hit this limit and had to back off). Middleware ordering is deliberate: `UseIpRateLimiting()` runs before `UseCors`/`UseAuthentication` so unauthenticated brute-force traffic is throttled pre-auth (`Program.cs:383` vs. 385-386).

**Found and fixed this session:** JWT access tokens previously embedded one JWT claim per permission code (`JwtTokenService.cs`). A role holding the full permission catalog (421 codes at the time) produced a ~16.8KB Authorization header — large enough to trip the default request-header-size limit on Node/Vite's dev server (HTTP 431 on every authenticated request past login) and to sit uncomfortably close to common reverse-proxy header-buffer limits (nginx's default 8KB, for one) in front of a real deployment. This was a real, live availability bug, not a theoretical one: it silently broke every page of the app beyond the dashboard for the account it affected. Fixed by packing all permission codes into a single gzip+base64url claim (`JwtPermissionsCodec.cs`, shared by `JwtTokenService` and `HttpCurrentUserContext`) — the same 421-permission token shrank from 16,829 to 5,033 characters. This buys significant headroom but does not remove the ceiling entirely: the underlying design still scales the token linearly with the size of a role's permission set, so a permission catalog that keeps growing (or a future customer defining unusually large custom roles) will eventually reapproach the same limit. The durable fix — moving to a reference-based permission model (JWT carries a role/version identifier; permissions are resolved server-side from a cached lookup) — is out of scope for this pass and tracked in `REMEDIATION_ROADMAP.md`.

### A.8.22 Segregation of networks
**Organizational/infra.** Not attestable from application code; a deployment-topology decision.

### A.8.23 Web filtering
**Not applicable.** FusionOS is a server-side ERP API + SPA, not an environment that filters outbound user web access.

### A.8.24 Use of cryptography
**Absent, and the most consequential gap in this analysis.** No field/column-level encryption exists anywhere — repo-wide search for `Encrypt|Cipher|Aes|DataProtection|KeyVault|KMS` under `backend/src` returns zero cryptography usage (the only two matches are unrelated `ConnectorConnection`/`OndcConnectorSyncAdapter` naming coincidences). Sensitive-data fields that a real deployment would need to protect (bank account numbers, GSTIN/PAN, payment credentials) are **largely not even modeled yet** as first-class fields — the one related entity, `GstComplianceSettings.cs`, is a settings toggle, not GSTIN storage. **No TLS configured on any database connection string.** **No secrets manager or KMS integration anywhere** — see the dedicated writeup in `ISMS_POLICIES.md` §Secrets Management for the installer- and Compose-specific plaintext-secret findings. `IntegrationHub`'s `ConnectorConnection.CredentialReference` is a deliberately-designed exception worth crediting: it stores only a *config-key name*, never a secret value (`ConnectorConnection.cs:14-31`) — the actual secret is read from `IConfiguration` at call time, which is still plaintext-at-rest but at least isn't duplicated into the database.

### A.8.25 Secure development life cycle
**Partial.** FluentValidation (backend) and parameterized EF Core queries are the consistent pattern across the codebase — no evidence of string-concatenated SQL found in this or prior session audits. But there's no documented secure-SDLC gate (e.g., mandatory security review checklist per PR, threat-modeling requirement for new modules) beyond the aspirational claim in `07_SECURITY.md` §11 — nothing in `.github/workflows/` enforces it.

### A.8.26 Application security requirements
**Partial.** Requirements exist informally (the `docs/blueprint/*.md` series) but aren't tied to a formal requirements-traceability process an auditor could inspect.

### A.8.27 Secure system architecture and engineering principles
**Implemented.** The modular-monolith architecture with per-module DbContexts, MediatR pipeline behaviors as centralized cross-cutting security enforcement (auth, tenant isolation, audit, now plan-entitlement), and the documented `IPubliclyAccessible`/`IRequirePermission` fail-closed pattern (`AuthorizationBehavior.cs:8-36`) is a genuinely sound, consistently-applied architecture. This is one of the strongest points in FusionOS's favor for a readiness review.

### A.8.28 Secure coding
**Implemented, with one caveat.** Parameterized queries via EF Core throughout; FluentValidation on all commands; webhook signature verification for Razorpay/Stripe/Shopify uses constant-time comparison (`CryptographicOperations.FixedTimeEquals`, `WebhooksController.cs:256-266`) — genuinely good practice. **Caveat:** the Windows installer's `GenerateSigningKey()` (`installer/FusionOS.iss:66-75`) uses Pascal's `Random()`, which is not cryptographically secure, to generate the JWT signing key for every on-prem install — a CSPRNG should be used here instead.

### A.8.29 Security testing in development and acceptance
**Partial.** Prior session work established real unit/integration test coverage across modules, but no dedicated security-test suite (fuzzing, dependency-CVE gate, penetration-test automation) — and as noted under A.8.8, the one automated vulnerability check that exists is explicitly non-blocking.

### A.8.30 Outsourced development
**Not applicable** for this codebase's current state (no evidence of outsourced components requiring separate assurance).

### A.8.31 Separation of development, testing and production environments
**Implemented.** `ASPNETCORE_ENVIRONMENT`-gated behavior throughout `Program.cs` (migration-on-boot restricted to Development/explicit `Deployment:AutoMigrate` opt-in; Swagger, detailed errors, and other dev conveniences gated the same way) plus separate `appsettings.Development.json`/`appsettings.json` files.

### A.8.32 Change management
**Organizational.** Governed by the team's PR/review process on the hosting platform, not something the codebase itself attests to.

### A.8.33 Test information
**Partial.** No evidence of production-data-derived test fixtures being masked/anonymized before use in lower environments — see A.8.11.

### A.8.34 Protection of information systems during audit testing
**Organizational.** No code-level control needed here; a process for scoping/authorizing future penetration tests, not an engineering gap.

---

## A.5 — Organizational controls (selected, code-relevant)

### A.5.16 Identity management
**Partial.** Single identity store (Core module's `User`/`Role`/`Permission` tables) per company, no federation with any external IdP. See A.8.5.

### A.5.17 Authentication information
**Partial.** Password policy is enforced only at the validator level (`RegisterUserValidator.cs` — minimum 10 characters per prior session notes); no breach-list checking (Have I Been Pwned k-anonymity API or equivalent), no forced periodic rotation policy, no account-lockout-after-N-failed-attempts beyond the general rate limiter.

### A.5.19–A.5.23 Supplier relationships
**Organizational.** IntegrationHub's connector abstraction (Razorpay/Stripe/Shopify/ONDC/Shipping providers) is a code-level integration point, but the *supplier security assessment process* itself (vendor risk questionnaires, contractual security clauses) is a procurement/legal function, not something in this repository.

### A.5.24–A.5.28 Incident management
**Partial.** No incident-response runbook exists in `docs/` today — `docs/DISASTER_RECOVERY.md` covers infrastructure DR, not a security-incident playbook (who's notified, containment steps, breach-notification timelines). This is a genuine, cheap-to-close gap — see `ISMS_POLICIES.md`.

### A.5.29–A.5.30 Business continuity
**Partial.** Covered partially by A.8.13/A.8.14 above — backup/restore is real but incomplete; no documented business-continuity test beyond the single restore drill exercised this session on a trivial dataset.

All remaining A.5 controls (information security policies, roles/responsibilities, screening, terms of employment, management commitment, etc.) are **organizational** — they describe a management system FusionOS the company must build (policies, training, contracts), not code FusionOS the product must run. See `ISMS_POLICIES.md` for the policy documents this readiness track produces to start closing those.

---

## A.6 — People controls

All 8 controls (screening, terms of employment, awareness/training, disciplinary process, responsibilities after termination, confidentiality agreements, remote working, event reporting) are **organizational** — HR/people-process controls with no source-code footprint. Tracked in `ISMS_POLICIES.md` and the remediation roadmap as management-system work, not engineering tickets.

## A.7 — Physical controls

All 14 controls (physical security perimeter, entry controls, protection against environmental threats, equipment security, clear desk/screen, etc.) are **organizational/hosting-provider** — for a cloud-hosted SaaS deployment these are largely inherited from the cloud provider's own ISO 27001 certification (AWS/Azure/GCP all hold one); for the on-prem Windows installer, physical security of the customer's own machine is explicitly the customer's responsibility per `docs/deployment/ONPREM_RUNBOOK.md`. Not code gaps.

---

## Cross-reference

- Remediation priorities and sequencing: `REMEDIATION_ROADMAP.md`
- Formal control-by-control applicability decision: `STATEMENT_OF_APPLICABILITY.md`
- New policy documents this analysis motivates: `ISMS_POLICIES.md`
