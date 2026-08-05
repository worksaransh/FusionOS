# ISO/IEC 27001 Readiness — FusionOS

This directory is the entry point for FusionOS's ISO 27001 readiness track. It exists to answer one question honestly: **how far is FusionOS from being able to pass an external ISO/IEC 27001:2022 certification audit, and what specifically closes the distance?**

## What's here

| Document | Purpose |
|---|---|
| [`GAP_ANALYSIS.md`](GAP_ANALYSIS.md) | Every Annex A control mapped against FusionOS's *actual* current implementation, with file:line evidence — not the aspirational design in `docs/blueprint/07_SECURITY.md`. Start here to understand what's real today. |
| [`STATEMENT_OF_APPLICABILITY.md`](STATEMENT_OF_APPLICABILITY.md) | The formal SoA document — the first thing an external auditor requests. All 93 Annex A controls, applicability decision, and status for each. |
| [`ISMS_POLICIES.md`](ISMS_POLICIES.md) | The core policy set an ISMS requires (Information Security, Access Control, Cryptography, Secure Development, Backup/DR, Incident Response, Acceptable Use, Supplier Security) — drafted to reflect FusionOS's real architecture, not generic boilerplate. |
| [`REMEDIATION_ROADMAP.md`](REMEDIATION_ROADMAP.md) | Every gap found, prioritized by cost/impact rather than by theme, sequenced into a real plan. |

## Executive summary

FusionOS has a **genuinely strong architectural foundation** for security — a consistently-applied MediatR pipeline-behavior pattern enforces authorization, tenant isolation, and audit logging centrally rather than scattered per-handler; RBAC fails closed by design; rate limiting is real and specifically hardened on the login endpoint; webhook signature verification is correctly implemented for three of four payment/commerce connectors. This is not a codebase starting from zero.

It also has **real, specific gaps that would fail a certification audit today**, the most consequential being:

1. No encryption at rest for sensitive data, and no TLS on database connections in any checked-in configuration.
2. No secrets manager anywhere — database passwords and signing keys live in plaintext config files, including one the Windows installer writes to disk on every customer install.
3. No second authentication factor.
4. Tenant isolation lives only in application code, with no database-level Row-Level Security as defense-in-depth.
5. Backups are real but unencrypted and — critically — **not actually scheduled to run automatically**, which means the documented Recovery Point Objective is aspirational rather than a real, testable number.

None of these are surprising for a platform at this stage, and none require throwing away existing work — they're additive. `REMEDIATION_ROADMAP.md` sequences them from a half-day fix (replacing an insecure random-number generator in the installer) through multi-week projects (SSO federation) that should be scoped as their own initiatives rather than squeezed into this pass.

## What this pass did NOT do

This pass is documentation and gap-analysis work, by design — the user explicitly chose this over starting the code-level remediation immediately, since several of the highest-value fixes (secrets manager choice, 2FA UX, SSO's target IdP) need product/business decisions this pass can't make unilaterally. No source code was changed as part of producing these four documents. The `GAP_ANALYSIS.md` findings are read-only observations; nothing was "fixed" to make the report look better.

## Status of this readiness track

- [x] Gap analysis against real, verified implementation state
- [x] Statement of Applicability drafted (all 93 controls)
- [x] Core ISMS policy set drafted
- [x] Remediation roadmap sequenced and prioritized
- [ ] Management review and formal approval of the above (roles currently unassigned — see the SoA's sign-off section)
- [ ] Tier 0/1 remediation items from the roadmap actually implemented
- [ ] Independent/external review scheduled
- [ ] Certification body engaged

This is a **readiness** track, not a certification. The gap between "readiness documentation exists" and "certified" is real engineering and organizational work — `REMEDIATION_ROADMAP.md` is the plan for closing it, not a claim that it's already closed.
