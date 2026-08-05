# ISO 27001 Remediation Roadmap

Sequenced by a simple rule: **fix what's cheap and high-impact first, regardless of theme** — a one-line CSPRNG fix and a multi-week KMS integration don't belong in the same priority tier just because they're both "cryptography." Effort is a rough engineering-days estimate for a single developer; items with an external dependency (a certificate, a vendor decision) are flagged as such and effort excludes that lead time.

Every item cites the control(s) it closes from `GAP_ANALYSIS.md` / `STATEMENT_OF_APPLICABILITY.md`, so progress on this roadmap is directly traceable back to the SoA.

---

## Tier 0 — Do immediately (cheap, no design decisions needed)

| Item | Controls closed | Effort | Notes |
|---|---|---|---|
| Replace the Windows installer's `GenerateSigningKey()` Pascal `Random()` with a CSPRNG | A.8.24, A.8.28 | < 0.5 day | `installer/FusionOS.iss` — swap to a cryptographically secure generator (e.g., shell out to `.NET`'s `RandomNumberGenerator` at build/install time, or use Inno Setup's own crypto-capable helper if available). Single highest risk-reduction-per-effort item in this whole roadmap. |
| Make `security-scan.yml`'s vulnerability checks blocking | A.8.8, A.8.29 | < 0.5 day | Remove `|| true` from both the `npm audit` and `dotnet list package --vulnerable` steps in `.github/workflows/security-scan.yml`. Expect this to surface existing vulnerable dependencies immediately — budget a follow-up pass to actually upgrade whatever it flags, not just flip the switch and let CI go red indefinitely. |
| Add TLS parameter to every checked-in Postgres connection string | A.8.20, A.8.24 | 0.5–1 day | `docker-compose.yml`, `appsettings.Development.json`, the installer's generated `appsettings.Production.json` template. Requires the Postgres container/server actually be configured to accept TLS — coordinate with whichever of the items below stands up real Postgres TLS certs (self-signed is acceptable for dev/on-prem; a real cert for SaaS). |
| Add CodeQL as a required PR check | A.8.29, A.8.25 | 0.5 day | GitHub-native, no new vendor account needed. Add `.github/workflows/codeql.yml` using the standard `github/codeql-action` starter for C# + JavaScript/TypeScript. |
| Draft and circulate the incident-response contact/reporting channel | A.5.24, A.5.26, A.6.8 | 0.5 day | Pure process — pick who the ISO/CISO-equivalent role is (even provisionally), publish where reports go. Unblocks closing A.5.24-28 as "documented" rather than "absent." |

## Tier 1 — Design + implement (days, not weeks; no external vendor blocker)

| Item | Controls closed | Effort | Notes |
|---|---|---|---|
| Add a scheduled backup job (cron/systemd timer/Task Scheduler depending on deployment tier) that actually invokes `backup-postgres.sh` | A.8.13, A.5.29-30 | 1–2 days | This is the difference between an aspirational RPO and a real one. Needs a per-tier decision: SaaS likely wants this as a scheduled task in the orchestration layer; on-prem installer should register a Windows Scheduled Task at install time. |
| Add an encryption step to the backup pipeline | A.8.13, A.7.10 | 1–2 days | Simplest version: pipe `pg_dump`'s output through `age` or `gpg` with a key the operator controls, store the encrypted blob. Needs a decision on where the encryption key itself lives (circular dependency on the secrets-management item below — sequence after it if possible, but a locally-held key is an acceptable interim state). |
| Backfill `AuditBehavior`'s `ChangesJson` with a real before/after diff | A.8.15, A.5.28 | 2–3 days | The column exists; the value is hard-coded `null`. Needs a generic mechanism to snapshot entity state before/after a command handler runs — likely a small addition to the pipeline behavior itself, comparing tracked EF Core entity property values pre/post `SaveChanges`. |
| Audit login attempts (success and failure) and register/refresh events | A.8.15, A.5.17, A.5.28 | 1–2 days | These are `IPubliclyAccessible` and currently produce zero audit trail. Add a narrower audit path specifically for auth events (can't reuse `IAuditableCommand`'s `CompanyId`/`UserId`-required shape as-is, since a failed login has no authenticated user — needs its own lightweight event shape). This is also the concrete fix that would let a future "alert on N failed logins" rule (A.8.16) have data to alert on. |
| Add a permission-catalog reconciliation job | A.8.2, A.5.18 | 1–2 days | Automates what this session did manually for Luxor's Owner role: on permission-catalog growth, backfill every company's Owner role (and optionally flag non-Owner roles for admin review) with newly-added permissions. Prevents the stale-role-drift finding from recurring at scale. |
| Add malware/content scanning to file uploads | A.8.7 | 2–4 days (+ vendor/library choice) | Document Management and OCR ingestion currently validate type/size only. A ClamAV sidecar (self-hosted, no per-scan cost) is the natural fit given the rest of the stack is self-hostable; a cloud API (e.g., a cloud provider's malware-scanning service) is the SaaS-tier alternative. |
| Add ONDC webhook signature verification | A.8.24, A.8.28 | 1 day | Bring the ONDC connector in line with the existing Razorpay/Stripe/Shopify pattern in `WebhooksController.cs` — check whether ONDC's protocol has an inbound webhook surface at all (the current implementation is poll-based sync); if it does, this is a direct copy of the existing pattern. |
| Add a hard-delete/data-purge mechanism with a retention-period policy | A.8.10, A.5.33, A.5.34 | 3–5 days | Needs a policy decision first (how long after contract termination is data purged — this is a legal/product decision, not purely engineering) then a mechanism (a scheduled job that hard-deletes soft-deleted rows past their retention window, company-by-company on contract termination). |

## Tier 2 — Needs a vendor/infrastructure decision, then implementation

| Item | Controls closed | Effort | Blocker |
|---|---|---|---|
| Secrets manager / KMS integration | A.8.24, A.5.17, the installer- and Compose-specific plaintext-secret findings | 3–5 days engineering, once the vendor is chosen | **Decision needed:** which secrets backend for which deployment tier? SaaS naturally wants the cloud provider's own KMS/Secrets Manager (AWS Secrets Manager, Azure Key Vault, GCP Secret Manager) matching whichever cloud FusionOS deploys to. On-prem is harder — a full HashiCorp Vault deployment is heavy for a single-machine installer; a lighter option (DPAPI-encrypted local file on Windows, or an OS keychain) is likely the right interim target. This is the single highest-value remaining item and should be scoped as its own mini-project, not squeezed into a sprint alongside other work. |
| Field-level encryption for sensitive data (bank accounts, GSTIN/PAN, payment credentials) | A.8.24, A.5.12, A.5.34 | 5–8 days, after a data-classification pass | **Blocker:** these fields are largely not yet modeled as first-class entities — this needs to be designed in at the same time the fields themselves are added (e.g., when TDS/TCS or a formal KYC/vendor-banking-details feature is built), not retrofitted. Depends on the secrets-manager item above for where the encryption keys live. |
| Asymmetric JWT signing (RS256/ES256) | A.8.5, A.8.24 | 3–5 days | **Blocker:** a live migration — every currently-issued token is HS256-signed; a cutover needs either a dual-verification window (accept both schemes during rollover) or accept forcing every active session to re-authenticate. Sequence this as a planned maintenance change, not a silent deploy. |
| 2FA/TOTP | A.8.5, A.5.17 | 5–8 days | No hard blocker, but meaningfully larger than it looks: needs enrollment UI, backup-code generation/storage, a recovery flow for lost devices, and a decision on whether it's mandatory or opt-in per company. Worth scoping as its own feature, not a compliance checkbox. |
| PostgreSQL Row-Level Security as defense-in-depth | A.8.3, A.8.24 | 5–10 days | Not blocked externally, but high risk of regression if done hastily — every existing query path needs to keep working under an RLS policy keyed on `CompanyId`, and the interaction with EF Core's connection-pooling (RLS session variables need to be set per-request, not per-connection) needs careful design. Recommend a spike/prototype on one module before rolling out platform-wide. |
| Installer/artifact code signing | A.8.19 | Engineering: 1 day. **Blocker: needs a code-signing certificate purchased/obtained** | Purely a procurement blocker, not a technical one — once a cert exists, wiring it into the build pipeline (`ISCC.exe`'s signing support, or a separate `signtool` step) is small. |
| SSO/OIDC federation | A.8.5, A.5.16 | 8–15 days | **Blocker:** needs a product decision on which IdPs to support first (Azure AD/Entra and Okta cover the most enterprise demand) and whether this is a per-company opt-in feature or platform-wide. Largest single item on this roadmap; treat as its own project with its own design doc, not a roadmap line item to squeeze in. |
| Reference-based JWT permission model (replace embedding permission codes in the token at all) | A.8.5, A.8.21 | 3–5 days | Not blocked externally. The gzip-consolidation fix landed this session (see `GAP_ANALYSIS.md` A.8.21) bought real headroom — 421 permissions went from a 16.8KB to a 5KB token — but the token size still scales with role size, so this recurs as the catalog keeps growing or a customer defines an unusually large custom role. The durable fix: JWT carries only a role identifier (or a permissions-version hash); `HttpCurrentUserContext` resolves the actual permission set from a cached, invalidated-on-`SetRolePermissionsCommand` lookup instead of decoding it from the token. Requires making the permission check path tolerant of a cache miss (fail open to a DB read) and is a genuine behavior change worth its own test pass, not a drop-in swap. |

## Tier 3 — Organizational / process (no code, but genuinely required for certification)

These block certification exactly as much as the code items above, but they're work for whoever holds the Information Security Officer role, not an engineering ticket:

- Assign the Information Security Officer / management-representative roles (unblocks A.5.2, A.5.35, and formal sign-off on `STATEMENT_OF_APPLICABILITY.md`).
- Approve the draft policies in `ISMS_POLICIES.md` through a real management review.
- Stand up a supplier-security assessment process for Razorpay/Stripe/Shopify/ONDC/shipping-provider relationships (A.5.19-22).
- Build a security-awareness training program, even a lightweight one (A.6.3).
- Schedule the first independent review of the ISMS (A.5.35) — this is normally what a certification-body Stage 1 audit itself becomes, but an internal dry-run first is standard practice.
- Formalize a data classification scheme (A.5.12) — this is a prerequisite for both the field-level-encryption item above and any meaningful data-masking policy (A.8.11).

---

## Suggested sequencing

1. **Tier 0** first, in one short sprint — it's cheap and removes the most embarrassing findings (an insecure RNG generating signing keys, a CI vulnerability gate that can't actually gate anything) before any external party looks at this.
2. **Tier 1** next — none of it is blocked, all of it is real engineering work with clear scope, and several items (audit-trail completeness, backup scheduling) directly strengthen the incident-response and business-continuity story Tier 3's organizational work needs to point to as evidence.
3. **Tier 2's secrets-manager item** should be pulled forward relative to the rest of Tier 2 — it's the prerequisite for the field-encryption and backup-encryption items, so doing it early avoids rework.
4. **Tier 2's remaining items** (2FA, RLS, asymmetric JWT, SSO, code signing) are each substantial enough to scope as individual projects, sequenced by actual customer/sales demand rather than this roadmap's own preference — e.g., if an enterprise prospect specifically requires SSO, that item jumps the queue.
5. **Tier 3** runs in parallel throughout, not after — the organizational work doesn't block on the code work finishing, and an external auditor will want to see the management system maturing alongside the technical controls, not bolted on at the end.
