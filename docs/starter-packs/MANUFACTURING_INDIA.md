# Starter Pack — Manufacturing (India)

**Status:** Ships as a script + JSON templates, run manually per company. Not yet wired into signup.
**Addresses:** the "ship a starter chart of accounts" action item from `docs/audit/luxor-2026/LUXOR_AUDIT_REPORT.md` — a brand-new FusionOS company currently starts with an empty Chart of Accounts, so nothing can post (invoice, journal entry, payroll run) until Finance is manually configured. This is the single biggest friction point in a self-serve trial.

## What it contains

A minimum-viable, Schedule III-aligned Chart of Accounts and GST tax-rate set for an Indian manufacturing company, seeded through the real Finance REST API — no direct database access, no EF migrations.

### Chart of Accounts (21 accounts)

Defined in [`scripts/starter-packs/manufacturing-india/chart-of-accounts.json`](../../scripts/starter-packs/manufacturing-india/chart-of-accounts.json).

| Code | Name | Type |
|------|------|------|
| 1100 | Cash | Asset |
| 1200 | Bank Accounts | Asset |
| 1300 | Accounts Receivable | Asset |
| 1410 | Inventory - Raw Materials | Asset |
| 1420 | Inventory - Finished Goods | Asset |
| 1500 | GST Input Tax Credit | Asset |
| 1600 | Fixed Assets - Plant & Machinery | Asset |
| 1650 | Accumulated Depreciation | Asset |
| 2100 | Accounts Payable | Liability |
| 2200 | GST Output Tax Payable | Liability |
| 2300 | Salaries Payable | Liability |
| 3100 | Share Capital | Equity |
| 3200 | Retained Earnings | Equity |
| 4100 | Sales Revenue - Domestic | Revenue |
| 4200 | Sales Revenue - Export | Revenue |
| 5100 | Cost of Goods Sold | Expense |
| 5200 | Raw Material Purchases | Expense |
| 5300 | Salaries & Wages | Expense |
| 5400 | Depreciation Expense | Expense |
| 5500 | Power & Fuel | Expense |
| 5600 | Freight & Forwarding | Expense |

`Type` is the exact, case-sensitive `AccountType` enum from `FusionOS.Modules.Finance.Domain.Accounts.AccountType`: `Asset`, `Liability`, `Equity`, `Revenue`, `Expense`. The enum has no contra-asset subtype, so Accumulated Depreciation is modeled as a plain `Asset` account, consistent with how the rest of this codebase's Finance module works today.

### GST Tax Jurisdiction + Tax Rates (1 jurisdiction, 12 rates)

Defined in [`scripts/starter-packs/manufacturing-india/tax-rates.json`](../../scripts/starter-packs/manufacturing-india/tax-rates.json).

One tax jurisdiction, `IN-GST` ("India - GST"), plus 12 tax rates covering all four standard GST slabs, split correctly across the three GST components:

| Slab | CGST (half, intra-state) | SGST (half, intra-state) | IGST (full, inter-state) |
|------|---------------------------|---------------------------|---------------------------|
| 5%   | 2.5%  (`CGST-5`)          | 2.5% (`SGST-5`)           | 5%  (`IGST-5`)            |
| 12%  | 6%    (`CGST-12`)         | 6%   (`SGST-12`)          | 12% (`IGST-12`)           |
| 18%  | 9%    (`CGST-18`)         | 9%   (`SGST-18`)          | 18% (`IGST-18`)           |
| 28%  | 14%   (`CGST-28`)         | 14%  (`SGST-28`)          | 28% (`IGST-28`)           |

`GstComponent` is the `FusionOS.Modules.Finance.Domain.TaxRates.GstComponent` enum (`NotApplicable`, `CGST`, `SGST`, `IGST`), parsed case-insensitively by `CreateTaxRateCommandHandler`.

## API contract these templates were written against

- `POST /api/v1/finance/accounts` — `CreateAccountRequest(Guid CompanyId, string Code, string Name, string AccountType, Guid? ParentAccountId)`, validated by `CreateAccountValidator` (Code ≤ 20 chars, Name ≤ 200 chars, AccountType must parse to the enum). See `backend/src/Modules/Finance/FusionOS.Modules.Finance.Api/Controllers/AccountsController.cs` and `backend/src/Modules/Finance/FusionOS.Modules.Finance.Application/Accounts/Commands/CreateAccount/`.
- `POST /api/v1/finance/tax-jurisdictions` — `CreateTaxJurisdictionRequest(Guid CompanyId, string Code, string Name)`. See `backend/src/Modules/Finance/FusionOS.Modules.Finance.Api/Controllers/TaxJurisdictionsController.cs`.
- `POST /api/v1/finance/tax-rates` — `CreateTaxRateRequest(Guid CompanyId, Guid TaxJurisdictionId, string Code, string Name, decimal Percentage, string? GstComponent)`, validated by `CreateTaxRateValidator` (Code ≤ 20 chars, Name ≤ 200 chars, Percentage between 0 and 100; `TaxJurisdictionId` must already exist for the company). See `backend/src/Modules/Finance/FusionOS.Modules.Finance.Application/TaxRates/Commands/CreateTaxRate/`.

## Running the seeding script

The script is [`scripts/seed-starter-pack-manufacturing-india.ps1`](../../scripts/seed-starter-pack-manufacturing-india.ps1). It calls the real Finance REST API (never the database directly) and is safe to re-run — duplicate `Code` values are rejected by the API and logged as `[INFO]`, not treated as fatal.

Unlike `scripts/seed-enterprise-tenant.ps1` (which logs in and, if needed, self-registers a company), this script expects a company and an authenticated user to already exist — it takes the `CompanyId` and a Bearer `Token` directly as parameters, since the intended use case is "run this against a company right after signup," not "spin up a demo tenant from scratch."

```powershell
./scripts/seed-starter-pack-manufacturing-india.ps1 `
    -CompanyId "11111111-2222-3333-4444-555555555555" `
    -Token "eyJhbGciOi..." `
    -BaseUrl "http://localhost:5000/api/v1"
```

Parameters:

| Parameter    | Required | Default                          | Notes                                                              |
|--------------|----------|-----------------------------------|---------------------------------------------------------------------|
| `-CompanyId` | Yes      | —                                  | The target company's GUID.                                        |
| `-Token`     | Yes      | —                                  | A valid JWT Bearer access token for a user with Finance write access on that company. Obtain it the same way `seed-enterprise-tenant.ps1` does: `POST {BaseUrl}/core/auth/login` with that user's email/password, and take `accessToken` from the response. |
| `-BaseUrl`   | No       | `http://localhost:5000/api/v1`    | Point at any environment's API base URL.                          |

What it does, in order:

1. Reads `scripts/starter-packs/manufacturing-india/chart-of-accounts.json` and `POST`s each account to `/finance/accounts`.
2. Reads `scripts/starter-packs/manufacturing-india/tax-rates.json`, `POST`s the `IN-GST` jurisdiction to `/finance/tax-jurisdictions` (falling back to a lookup by `Code` if it already exists from a prior run), then `POST`s all 12 GST tax rates to `/finance/tax-rates` using that jurisdiction's id.

Each step logs `[OK]` (green) on success and `[INFO]` (gray) on a per-item failure (e.g. "already exists") without aborting the rest of the run — the same convention as `scripts/seed-enterprise-tenant.ps1`.

## Not done in this pass

This pack is deliberately scoped to Chart of Accounts + GST tax rates only, matching the audit's specific "ship a starter chart of accounts" callout. Not included, and left for a future pass if this pattern is extended to other verticals: BOM templates, tax rates for other regimes (VAT, US sales tax, etc.), and any automatic "apply a starter pack" step during company signup — today this is an explicit, manually-run script.
