# Statement of Applicability — ISO/IEC 27001:2022 Annex A

**Organization:** FusionOS
**Scope of the ISMS:** the FusionOS ERP platform — its source code, build/release pipeline, SaaS and on-prem deployment artifacts, and the data it processes on behalf of customer companies.
**Version:** Draft 1 — first pass produced as part of the ISO 27001 readiness track. Not yet reviewed/approved by a designated Information Security Officer or management representative — see `README.md` for sign-off status.

This document records, for every control in Annex A of ISO/IEC 27001:2022, whether it is **applicable** to FusionOS's ISMS scope, the **justification** for that decision, and a pointer to current **implementation status**. It is the formal deliverable an external auditor requests first. Detailed evidence for every "Implemented"/"Partial"/"Absent" status below lives in `GAP_ANALYSIS.md`; this document should not duplicate that evidence, only reference it.

**All 93 controls are marked Applicable.** FusionOS is a multi-tenant ERP handling financial, HR, and operational data for customer businesses — no Annex A control is genuinely out of scope for that risk profile, including physical controls (relevant to both cloud-hosting-provider inheritance and the on-prem installer's customer premises) and people controls (relevant the moment FusionOS has employees or contractors, which it does). Marking a control "Not Applicable" prematurely is the single most common finding auditors flag against a young ISMS — better to mark everything applicable and show a real, honest implementation-status distribution than to exclude controls that will need revisiting anyway.

---

## A.5 Organizational controls (37)

| # | Control | Applicable | Justification | Status |
|---|---|---|---|---|
| 5.1 | Policies for information security | Yes | ISMS requires a documented policy set | Partial — `ISMS_POLICIES.md` drafted this pass, not yet management-approved |
| 5.2 | Information security roles and responsibilities | Yes | Required for any ISMS | Absent — no ISO-facing role (ISO/CISO-equivalent) formally assigned yet |
| 5.3 | Segregation of duties | Yes | Directly relevant — FusionOS *ships* SoD as a product feature | Implemented (product feature) / Partial (internal engineering org) — see `GAP_ANALYSIS.md` A.8.2; internal dev/ops segregation not yet documented |
| 5.4 | Management responsibilities | Yes | Required for any ISMS | Absent — no management review cadence established yet |
| 5.5 | Contact with authorities | Yes | Relevant once handling real customer PII/financial data | Absent — no documented contact list (data-protection authority, CERT, etc.) |
| 5.6 | Contact with special interest groups | Yes | Standard practice (threat intel sharing) | Absent |
| 5.7 | Threat intelligence | Yes | Feeds vulnerability management (A.8.8) | Partial — Dependabot is passive threat intel; no active feed subscription |
| 5.8 | Information security in project management | Yes | Every module build in this repo is a project | Partial — security review is a documented aspiration (`07_SECURITY.md` §11) not a real gate |
| 5.9 | Inventory of information and other associated assets | Yes | Required baseline | Partial — the module/entity structure is de facto documented (blueprint docs, this gap analysis) but no formal asset register exists |
| 5.10 | Acceptable use of information and other associated assets | Yes | Required | Partial — `ISMS_POLICIES.md` drafts an Acceptable Use Policy this pass |
| 5.11 | Return of assets | Yes | Relevant on employee/contractor offboarding | Organizational — process not yet documented |
| 5.12 | Classification of information | Yes | Relevant — financial/HR/tax data needs classification | Absent — no data classification scheme exists (see A.5.34, A.8.11 gaps) |
| 5.13 | Labelling of information | Yes | Depends on 5.12 | Absent |
| 5.14 | Information transfer | Yes | Relevant to API/webhook integrations | Partial — TLS-in-transit is a deployment expectation, not enforced in checked-in config (see `GAP_ANALYSIS.md` A.8.20) |
| 5.15 | Access control | Yes | Core to the product | Implemented — RBAC via `AuthorizationBehavior` (see A.8.2/A.8.3) |
| 5.16 | Identity management | Yes | Core to the product | Partial — see `GAP_ANALYSIS.md` A.5.16 |
| 5.17 | Authentication information | Yes | Core to the product | Partial — see `GAP_ANALYSIS.md` A.5.17 (no breach-list check, no 2FA) |
| 5.18 | Access rights | Yes | Core to the product | Implemented, with a caveat — see `GAP_ANALYSIS.md` A.8.2 (role staleness) |
| 5.19 | Information security in supplier relationships | Yes | IntegrationHub connects to Razorpay/Stripe/Shopify/ONDC/shipping providers | Organizational — no formal supplier security assessment process yet |
| 5.20 | Addressing information security within supplier agreements | Yes | Same suppliers as 5.19 | Organizational — contractual, not code |
| 5.21 | Managing information security in the ICT supply chain | Yes | NuGet/npm dependency chain | Partial — Dependabot exists; no SBOM generation |
| 5.22 | Monitoring, review and change management of supplier services | Yes | Same suppliers as 5.19 | Organizational |
| 5.23 | Information security for use of cloud services | Yes | SaaS deployment tier runs on cloud infrastructure | Organizational — inherited from cloud provider's own ISO 27001 certification; not independently verified by FusionOS |
| 5.24 | Information security incident management planning and preparation | Yes | Required | Absent — no incident-response runbook exists (see `GAP_ANALYSIS.md` A.5.24-28); drafted this pass in `ISMS_POLICIES.md` |
| 5.25 | Assessment and decision on information security events | Yes | Follows from 5.24 | Absent |
| 5.26 | Response to information security incidents | Yes | Follows from 5.24 | Absent |
| 5.27 | Learning from information security incidents | Yes | Follows from 5.24 | Absent — no incident has occurred to learn from, but no process exists for when one does |
| 5.28 | Collection of evidence | Yes | Follows from 5.24 | Partial — audit logging (A.8.15) provides some forensic material, with the gaps noted there |
| 5.29 | Information security during disruption | Yes | Relevant to DR posture | Partial — see `GAP_ANALYSIS.md` A.8.13/14 |
| 5.30 | ICT readiness for business continuity | Yes | Same as 5.29 | Partial — one restore drill exercised on trivial data; not a repeatable, scheduled BC test |
| 5.31 | Legal, statutory, regulatory and contractual requirements | Yes | FusionOS handles GST/tax-compliance data directly | Partial — GST e-invoicing/e-way-bill/TDS-TCS domains exist in-product; no formal legal-register process for the ISMS itself |
| 5.32 | Intellectual property rights | Yes | Standard | Organizational |
| 5.33 | Protection of records | Yes | Financial records retention is core to an ERP | Partial — soft-delete preserves records (A.8.10) but no formal retention-period policy exists |
| 5.34 | Privacy and protection of PII | Yes | HR/CRM modules hold PII | Partial — no data classification (5.12) or field-level protection (A.8.24) yet for PII specifically |
| 5.35 | Independent review of information security | Yes | Required for certification | Absent — this document is the first internal review; no independent/external review has occurred |
| 5.36 | Compliance with policies, rules and standards for information security | Yes | Required | Absent — no compliance-monitoring process exists yet (policies themselves are only just drafted this pass) |
| 5.37 | Documented operating procedures | Yes | Required | Partial — `docs/deployment/ONPREM_RUNBOOK.md` and the backup/restore scripts are real documented procedures; broader ops runbooks (incident response, access review) are gaps closed by this pass's `ISMS_POLICIES.md` |

## A.6 People controls (8)

| # | Control | Applicable | Justification | Status |
|---|---|---|---|---|
| 6.1 | Screening | Yes | Standard hiring control | Organizational |
| 6.2 | Terms and conditions of employment | Yes | Standard | Organizational |
| 6.3 | Information security awareness, education and training | Yes | Required | Absent — no training program exists |
| 6.4 | Disciplinary process | Yes | Standard | Organizational |
| 6.5 | Responsibilities after termination or change of employment | Yes | Standard, relevant given RBAC/role model exists to revoke access through | Partial — the product supports deactivating a `User`/revoking permissions (`core.user.deactivate`); the *process* of triggering that on offboarding is organizational, not enforced by code |
| 6.6 | Confidentiality or non-disclosure agreements | Yes | Standard | Organizational |
| 6.7 | Remote working | Yes | Relevant — this is a remote-buildable codebase | Organizational |
| 6.8 | Information security event reporting | Yes | Feeds 5.24-28 | Absent — no reporting channel documented yet |

## A.7 Physical controls (14)

| # | Control | Applicable | Justification | Status |
|---|---|---|---|---|
| 7.1 | Physical security perimeters | Yes | SaaS: inherited from cloud provider. On-prem: customer's premises. | Organizational |
| 7.2 | Physical entry | Yes | Same as 7.1 | Organizational |
| 7.3 | Securing offices, rooms and facilities | Yes | Same as 7.1 | Organizational |
| 7.4 | Physical security monitoring | Yes | Same as 7.1 | Organizational |
| 7.5 | Protecting against physical and environmental threats | Yes | Same as 7.1 | Organizational |
| 7.6 | Working in secure areas | Yes | Same as 7.1 | Organizational |
| 7.7 | Clear desk and clear screen | Yes | Standard | Organizational |
| 7.8 | Equipment siting and protection | Yes | Same as 7.1 | Organizational |
| 7.9 | Security of assets off-premises | Yes | Relevant to remote work (6.7) | Organizational |
| 7.10 | Storage media | Yes | Relevant to backup media (A.8.13) | Partial — backup files are unencrypted local files (see `GAP_ANALYSIS.md` A.8.13); no documented media-handling policy |
| 7.11 | Supporting utilities | Yes | Same as 7.1 | Organizational |
| 7.12 | Cabling security | Yes | Same as 7.1 | Organizational |
| 7.13 | Equipment maintenance | Yes | Same as 7.1 | Organizational |
| 7.14 | Secure disposal or re-use of equipment | Yes | Relevant to decommissioned on-prem installs | Organizational |

## A.8 Technological controls (34)

| # | Control | Applicable | Justification | Status |
|---|---|---|---|---|
| 8.1 | User endpoint devices | Yes | SaaS accessed via any browser (no MDM control point); on-prem installer runs on the customer's own Windows machine | Organizational — no endpoint-management control exists in-product, appropriately, since FusionOS doesn't manage customer endpoints |
| 8.2 | Privileged access rights | Yes | Core to the product | Partial — see `GAP_ANALYSIS.md` |
| 8.3 | Information access restriction | Yes | Core to the product | Partial — see `GAP_ANALYSIS.md` (app-layer only, no DB-level RLS) |
| 8.4 | Access to source code | Yes | Standard | Organizational (Git hosting platform) |
| 8.5 | Secure authentication | Yes | Core to the product | Partial — see `GAP_ANALYSIS.md` (no 2FA, symmetric JWT signing) |
| 8.6 | Capacity management | Yes | Relevant to availability | Absent — see `GAP_ANALYSIS.md` |
| 8.7 | Protection against malware | Yes | Relevant to file uploads (Documents, OCR ingestion) | Absent — see `GAP_ANALYSIS.md` |
| 8.8 | Management of technical vulnerabilities | Yes | Standard | Partial — see `GAP_ANALYSIS.md` (Dependabot yes, blocking SAST no) |
| 8.9 | Configuration management | Yes | Relevant to multi-tier deployment (SaaS/Docker/on-prem) | Partial — see `GAP_ANALYSIS.md` |
| 8.10 | Information deletion | Yes | Core to the product's data lifecycle | Partial — soft-delete only, no hard-purge/retention enforcement |
| 8.11 | Data masking | Yes | Relevant to lower environments and audit logs | Absent |
| 8.12 | Data leakage prevention | Yes | Standard | Absent |
| 8.13 | Information backup | Yes | Core to the product's DR story | Partial — see `GAP_ANALYSIS.md` (unencrypted, unscheduled) |
| 8.14 | Redundancy of information processing facilities | Yes | Relevant to availability | Organizational/deployment-tier decision |
| 8.15 | Logging | Yes | Core to the product's audit story | Partial — see `GAP_ANALYSIS.md` (no field diff, no query/login audit) |
| 8.16 | Monitoring activities | Yes | Observability stack exists | Partial — see `GAP_ANALYSIS.md` (no security-specific alerting) |
| 8.17 | Clock synchronization | Yes | Standard | Organizational |
| 8.18 | Use of privileged utility programs | Yes | Standard | Organizational |
| 8.19 | Installation of software on operational systems | Yes | The Windows installer is exactly this control's subject | Partial — see `GAP_ANALYSIS.md` (unsigned installer) |
| 8.20 | Networks security | Yes | Core to the product's deployment | Partial — see `GAP_ANALYSIS.md` (CORS real, DB TLS absent) |
| 8.21 | Security of network services | Yes | Core to the product | Implemented — rate limiting, see `GAP_ANALYSIS.md` |
| 8.22 | Segregation of networks | Yes | Relevant to deployment topology | Organizational |
| 8.23 | Web filtering | No | FusionOS is a server-side API + SPA; it does not filter outbound user web traffic | N/A |
| 8.24 | Use of cryptography | Yes | The single most consequential gap in this analysis | Absent — see `GAP_ANALYSIS.md` |
| 8.25 | Secure development life cycle | Yes | Standard | Partial — see `GAP_ANALYSIS.md` |
| 8.26 | Application security requirements | Yes | Standard | Partial — see `GAP_ANALYSIS.md` |
| 8.27 | Secure system architecture and engineering principles | Yes | Standard | Implemented — see `GAP_ANALYSIS.md` (a genuine strength) |
| 8.28 | Secure coding | Yes | Standard | Implemented, with one caveat — see `GAP_ANALYSIS.md` (installer CSPRNG gap) |
| 8.29 | Security testing in development and acceptance | Yes | Standard | Partial — see `GAP_ANALYSIS.md` |
| 8.30 | Outsourced development | No | No outsourced development component identified in the current codebase | N/A — revisit if this changes |
| 8.31 | Separation of development, testing and production environments | Yes | Core to the product's deployment | Implemented — see `GAP_ANALYSIS.md` |
| 8.32 | Change management | Yes | Standard | Organizational (PR/review process) |
| 8.33 | Test information | Yes | Relevant to lower-environment data handling | Partial — see `GAP_ANALYSIS.md` |
| 8.34 | Protection of information systems during audit testing | Yes | Relevant once penetration testing begins | Organizational — no pen-test has occurred yet to protect against |

---

## Notes on the two "Not Applicable" determinations

- **8.23 Web filtering** — genuinely N/A. FusionOS has no role in filtering what websites its own users (customers' staff) browse; it is not a network-perimeter or endpoint-security product. Revisit only if FusionOS were to add, e.g., an embedded browser component with its own content-filtering requirement — it does not today.
- **8.30 Outsourced development** — marked N/A because no outsourced/contracted development work was identified during this pass. This determination should be revisited the moment any component is built by a third party (contractor, agency, acquired module) — at that point 8.30 becomes Applicable and needs its own assessment.

Every other control is Applicable by design — see the scope rationale above.

## Sign-off

| Role | Name | Date | Signature |
|---|---|---|---|
| Information Security Officer (or equivalent) | *unassigned* | | |
| Management representative | *unassigned* | | |

This SoA is a **draft** until both roles above are assigned and this document is formally reviewed. See `README.md` for what "done" looks like for this readiness track.
