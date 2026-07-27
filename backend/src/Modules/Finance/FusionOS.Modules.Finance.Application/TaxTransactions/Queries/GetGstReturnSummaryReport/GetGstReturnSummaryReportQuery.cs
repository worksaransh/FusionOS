using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxTransactions.Queries.GetGstReturnSummaryReport;

/// <summary>
/// GST filing support (2026-07-22). Groups the period's TaxTransaction rows
/// by their TaxRate's GstComponent (CGST/SGST/IGST) and Direction
/// (Output = collected on sales, Input = paid on purchases, i.e. input tax
/// credit), producing the output-tax/input-credit/net-payable breakdown a
/// GSTR-1/GSTR-3B filing needs. Reuses finance.tax-rate.read — same
/// established pattern as GetTaxPeriodSummaryReportQuery, since this only
/// reads tax master data plus Finance's own TaxTransaction records.
/// </summary>
public sealed record GetGstReturnSummaryReportQuery(Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : IQuery<GstReturnSummaryReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.read" };
}
