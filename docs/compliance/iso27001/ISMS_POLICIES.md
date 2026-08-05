# FusionOS Information Security Management System — Core Policies

**Status:** Draft 1, produced as part of the ISO 27001 readiness track. Each policy below states what *must* be true — where current implementation falls short, that's a finding in `GAP_ANALYSIS.md` and a line item in `REMEDIATION_ROADMAP.md`, not a reason to water down the policy itself. Policies should describe the target state; the roadmap is what closes the distance to it.

**Approval:** none of these policies are yet approved by a management representative. They are a starting draft for that review, not a substitute for it. See `README.md`.

---

## 1. Information Security Policy

FusionOS treats the confidentiality, integrity, and availability of every customer company's data as the platform's core obligation — this is a multi-tenant ERP holding financial ledgers, payroll, and operational records for businesses that have no direct visibility into how their data is protected once it's in FusionOS's systems.

**Principles:**
- Security is enforced in code, not left to documentation or developer discipline alone — the existing pattern of centralizing authorization, tenant isolation, and audit logging into MediatR pipeline behaviors (`AuthorizationBehavior`, `TenantIsolationBehavior`, `AuditBehavior`) rather than scattering checks across individual handlers is the model every future cross-cutting security control should follow.
- Every new module or externally-facing endpoint requires an explicit security review before release, covering: authentication/authorization correctness, input validation coverage, audit log completeness, tenant-isolation correctness for any new table, and — new as of this policy — a cryptography/secrets review for any newly-introduced sensitive field.
- No control in this policy set is satisfied by a design document alone. "Designed for" and "implemented" are different states, and this ISMS's gap analysis exists specifically to keep that distinction honest (see `GAP_ANALYSIS.md`'s explicit calling-out of `docs/blueprint/07_SECURITY.md`'s aspirational claims).
- This policy is reviewed at least annually, or immediately after any security incident (see §6, Incident Response Policy).

**Scope:** all FusionOS source code, build/release infrastructure, and both SaaS and on-premises deployment artifacts.

---

## 2. Access Control Policy

**Principle of least privilege, enforced twice.** Every request into the system must resolve to an explicit permission check or an explicit public-access declaration — there is no third, unmarked state. This is not aspirational; it is how `AuthorizationBehavior` already works today (`backend/src/Shared/FusionOS.BuildingBlocks.Application/Behaviors/AuthorizationBehavior.cs`): a request implementing neither `IRequirePermission` nor `IPubliclyAccessible` throws, rather than silently passing through. Every new command or query added to the codebase must implement one of the two.

**Tenant isolation is access control.** A user's permissions are scoped to their own company; `TenantIsolationBehavior` enforces that a request's `CompanyId` matches the caller's own. This application-layer control is currently the *only* layer (see `GAP_ANALYSIS.md` A.8.3) — this policy requires that gap be closed with database-level Row-Level Security as defense-in-depth, so a bug in the application layer alone cannot leak cross-company data. Until RLS lands, every new query path must be reviewed specifically for whether it could bypass `TenantIsolationBehavior` (e.g., a raw SQL query, a report handler that doesn't carry `CompanyId` on its request).

**Role and permission hygiene.** A company's Owner role is granted every permission in the catalog at creation time. As the permission catalog grows (new modules, new commands), this policy requires a mechanism to keep existing roles in sync with the current catalog — this session's own live testing found Owner-role permissions drifting stale relative to a growing catalog (421 permissions in the catalog, 395 actually granted to a pre-existing company's Owner role). Until an automated reconciliation job exists, this is a manual step required whenever the permission catalog changes, and should be tracked as a remediation item (see `REMEDIATION_ROADMAP.md`).

**Privileged access review.** Companies' Owner-role membership and any custom role holding `core.role.manage` should be reviewed periodically (target: quarterly, once a customer base exists to review). No automated recertification exists today; this is a manual process requirement until one is built.

**User lifecycle.** Deactivating a user (`core.user.deactivate`) must be the immediate action on any offboarding, whether that's a FusionOS customer's employee or (once FusionOS itself has staff) FusionOS's own personnel accessing any internal tooling.

---

## 3. Cryptography and Secrets Management Policy

This is the policy area with the largest gap between requirement and current implementation (see `GAP_ANALYSIS.md` A.8.24) — stated plainly here so the roadmap has an unambiguous target.

**Data in transit.** All network traffic — client-to-API, API-to-database, API-to-external-integration — must use TLS 1.2 or later. No checked-in configuration (Docker Compose, `appsettings.*.json`, the Windows installer) may specify or default to an unencrypted database connection. This is currently violated (no `sslmode`/`Ssl Mode` parameter exists in any checked-in Postgres connection string) and is the top item in the remediation roadmap.

**Data at rest.** Fields classified as sensitive under a future data-classification exercise (§ referencing A.5.12 in the SoA) — at minimum bank account numbers, GSTIN/PAN and equivalent tax identifiers, and payment credentials — must be encrypted at the column level using envelope encryption, with the data key itself protected by a KMS-managed master key. No sensitive field may be stored in plaintext once classified. Today, these fields are largely not yet even modeled as first-class entities, which means this requirement has to be designed in at the same time those entities are built, not retrofitted after the fact.

**Secrets never live in source control or plaintext config at rest.** Database credentials, the JWT signing key, and every external-integration API key/webhook secret must be resolved at runtime from a secrets manager (a cloud KMS/Secrets Manager for SaaS; a local encrypted vault such as HashiCorp Vault, or at minimum the OS's own credential store, for on-premises). This is currently violated in three places, all documented in `GAP_ANALYSIS.md` A.8.24/A.5.17: the Windows installer writes the JWT signing key and database password into a plaintext `appsettings.Production.json`; Docker Compose passes a hardcoded database password as a plain environment variable; and `IntegrationHub` connector secrets are resolved via plain `IConfiguration` lookups. Each of these needs a dedicated remediation item — they are not the same fix (an installer-generated local file needs different treatment than a cloud-hosted secrets manager).

**Key generation.** Any cryptographic key or secret this platform generates — the JWT signing key foremost — must be generated with a cryptographically secure random number generator. The Windows installer's current use of Pascal's `Random()` (`installer/FusionOS.iss`) is a direct violation of this policy and should be treated as a priority fix, not a "someday" item, since it's a single-line change with an outsized risk reduction.

**JWT signing.** Access tokens should be signed asymmetrically (RS256 or ES256) so that services which only need to *validate* tokens never hold the key capable of *minting* them. The current shared-secret HS256 scheme is a policy violation carried forward from before this ISMS existed; migrating it requires a coordinated rollout (old tokens must remain valid through the transition) and is scoped accordingly in the roadmap.

---

## 4. Secure Development Policy

**Input handling.** Every command and query must validate its inputs via FluentValidation (backend) — this is already the consistent pattern across the codebase and this policy simply makes it a requirement rather than a convention. All database access must go through parameterized EF Core queries; raw/string-concatenated SQL is prohibited without an explicit, documented, reviewed exception.

**Dependency hygiene.** Automated dependency-update proposals (Dependabot) are necessary but not sufficient. A build must **fail**, not merely warn, when a dependency scan finds a high-or-critical vulnerability. The current CI configuration (`security-scan.yml`'s `|| true` on both the npm and NuGet vulnerability checks) is a policy violation — every vulnerability check that exists today is informational-only, which is functionally equivalent to not having the gate at all from a risk-acceptance standpoint. This must change to a blocking check before this policy can be considered satisfied.

**Static analysis.** A SAST tool (CodeQL, at minimum, given free GitHub-native integration) must run on every pull request. None exists today.

**Webhook and inbound-integration security.** Every inbound webhook receiver must verify the sending provider's signature before processing any payload, using constant-time comparison — the existing Razorpay/Stripe/Shopify implementations in `WebhooksController.cs` are the correct pattern and the template for any new connector (ONDC currently lacks this and must be brought in line — see `GAP_ANALYSIS.md` A.8.24/§11 of the earlier audit).

**Code signing.** Any installable artifact FusionOS distributes (the Windows installer, and any future macOS/mobile package) must be code-signed once a signing certificate is available, so customers have supply-chain assurance that what they installed is what FusionOS built. Unsigned distribution is accepted only as an interim/no-budget-for-certificate state, not a permanent one.

---

## 5. Backup and Disaster Recovery Policy

**Backups must be automated, not manual.** A backup that depends on a human remembering to run a script has no real Recovery Point Objective — this policy requires a scheduled job (cron, a scheduled task, or equivalent) triggering `scripts/backup-postgres.sh` on a defined cadence appropriate to the deployment tier, with the cadence itself documented as the RPO commitment for that tier.

**Backups must be encrypted at rest**, both in the local retention window and in any offsite/replicated copy. `pg_dump`'s custom-format output has no encryption of its own; this policy requires an encryption step (e.g., `gpg`/`age`/cloud-provider server-side encryption on the storage target) be added to the backup pipeline before backups are considered compliant.

**Restore must be tested on a schedule, not just once.** This session exercised a single restore drill on a trivial dev-scale dataset — a real value for RTO/RPO cannot be claimed from that alone. This policy requires the restore drill be repeated periodically (target: quarterly) against a production-representative data volume, with the measured RTO updated in `docs/deployment/ONPREM_RUNBOOK.md` after each drill, not left as a one-time number.

**Offsite copy required.** A backup that lives only on the same host as the database it protects does not protect against host loss. At least one copy must be replicated to separate storage (a different cloud region/account, or for on-prem, a separate physical location) once such a target exists.

---

## 6. Incident Response Policy

FusionOS has no documented security-incident process today (`GAP_ANALYSIS.md` A.5.24-28) — this section is a first draft of one, to be expanded as the ISMS matures.

**Detection.** Any of the following constitutes a reportable security event: evidence of unauthorized cross-tenant data access, a credential/secret suspected to be exposed (e.g., accidentally committed, found in a public location), an anomalous spike in failed logins or permission-denied responses, or a report from any external party (customer, security researcher) alleging a vulnerability.

**Reporting.** Until a dedicated intake channel exists, security reports and internally-discovered incidents are routed to whoever holds the Information Security Officer role assignment (currently unassigned — see `STATEMENT_OF_APPLICABILITY.md`). This policy requires that assignment be made and a public-facing responsible-disclosure contact be published before FusionOS has any external customers relying on it in production.

**Response.** On confirmation of an incident: (1) contain — this may mean revoking a compromised credential, deactivating a user, or rolling the JWT signing key (which, given the current shared-secret HS256 scheme, invalidates every issued token — a real operational cost the asymmetric-signing remediation item addresses); (2) assess scope using the audit log (`AuditBehavior`'s captured `EntityType`/`EntityId`/`ActorId`/`CompanyId` trail — noting the current gap that queries and login attempts aren't captured, which will limit forensic reconstruction until that's fixed); (3) notify affected customer companies per applicable legal/contractual obligations; (4) remediate the root cause; (5) document what happened.

**Learning.** Every incident, once resolved, produces a written retrospective — what happened, why existing controls didn't prevent it (or did, and worked as intended), and what changes (code, policy, or process) follow from it. This retrospective feeds back into `GAP_ANALYSIS.md` as a new finding if it reveals a control gap not already tracked.

---

## 7. Acceptable Use Policy

Anyone with access to FusionOS's source code, infrastructure, or production data (employees, contractors, and — for the product itself — customer users within their own company's data) must:

- Use access only for its intended purpose — a developer's production database access is for operating the platform, not for browsing customer data out of curiosity.
- Never copy production data (including backups) to a personal device or an unmanaged location.
- Never commit secrets, credentials, or customer data to source control — even in a private repository, even temporarily "to fix later."
- Report suspected security weaknesses through the incident-response channel (§6), not by exploiting them further "to see how far it goes."
- For customer-facing use of the product itself: each user's actions are bound by the RBAC permissions their company's administrator has granted them — the product enforces this technically (`AuthorizationBehavior`), and this policy states the expectation that users not attempt to circumvent it (e.g., sharing credentials to work around a permission boundary).

---

## 8. Third-Party and Supplier Security Policy

FusionOS integrates with external providers through the IntegrationHub module — payment gateways (Razorpay, Stripe), e-commerce platforms (Shopify), ONDC, and shipping providers, with more connectors added over time.

**Before adding a new connector:** the integration must use the existing `ConnectorConnection.CredentialReference` pattern (storing a config-key *name*, never a secret value, in the database — `ConnectorConnection.cs`) rather than inventing a new credential-storage approach per connector. Any inbound webhook the connector receives must implement signature verification before this policy considers the connector complete (see §4).

**Ongoing.** Each supplier relationship should have a documented security contact and, where the relationship is significant enough (payment processing, in particular), a periodic review of that supplier's own security posture (SOC 2 report, ISO 27001 certificate, or equivalent) — this is currently not tracked anywhere and is a process gap this policy identifies rather than resolves; resolving it is procurement/legal work, not an engineering task.

**Data shared with suppliers** must be the minimum necessary for the integration to function — e.g., a shipping connector needs delivery addresses, not full order financial detail, unless the integration specifically requires it.

---

## Cross-reference

- Evidence backing every "currently violated" statement above: `GAP_ANALYSIS.md`
- Formal control applicability: `STATEMENT_OF_APPLICABILITY.md`
- Sequencing and effort for closing the gaps these policies name: `REMEDIATION_ROADMAP.md`
