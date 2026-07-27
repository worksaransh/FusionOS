using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetTaxPeriodSummaryReport;

/// <summary>
/// Tax period summary — the report a Finance user needs to actually file a
/// GST/VAT return: every TaxRate, grouped under its TaxJurisdiction, for a
/// given filing period.
///
/// <b>Investigated (2026-07-22) and originally scoped down:</b> at the time,
/// a real filing report showing, per jurisdiction/rate, total output tax
/// collected (from Sales invoices) and total input tax paid (from
/// Procurement purchase orders) for the period was NOT reachable by Finance:
/// neither the InvoiceIssued nor PurchaseOrderGoodsReceiptCosted integration
/// event carried a TaxRateId or TaxAmount, only an all-in total, so Finance's
/// own schema had zero tax-amount data of any kind to summarize by period.
///
/// <b>That gap has since been closed (2026-07-22 GST follow-up):</b> both
/// event payloads were extended to carry tax data, Finance now persists it as
/// TaxTransaction rows, and <c>GetGstReturnSummaryReportQuery</c> is the real
/// replacement report — it groups those TaxTransaction rows by GstComponent
/// and Direction to produce genuine output-tax/input-credit/net-payable
/// figures for a GSTR-1/GSTR-3B filing. This older query is kept only as a
/// plain tax rate/jurisdiction register (no period amounts) — prefer
/// <c>GetGstReturnSummaryReportQuery</c> for anything that needs actual
/// filed amounts. The output/input tax fields here remain deliberately null
/// rather than a fabricated zero (see TaxPeriodSummaryLineDto's doc comment),
/// since this query still does not compute them.
///
/// Gated by the same finance.tax-rate.read permission CalculateLineTaxQuery
/// uses: it only reads TaxRate/TaxJurisdiction master data.
/// </summary>
public sealed record GetTaxPeriodSummaryReportQuery(Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : IQuery<TaxPeriodSummaryReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.read" };
}
