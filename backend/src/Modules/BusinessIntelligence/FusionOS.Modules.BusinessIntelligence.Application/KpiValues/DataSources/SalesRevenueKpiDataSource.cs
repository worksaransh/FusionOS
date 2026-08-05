using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Sales.Application.Commissions.Queries.GetSalesCommissionSummaryReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "total sales revenue" KPI (Phase 2.3). Create a KpiDefinition with
/// Code = "SALES_REVENUE" to wire it up.
///
/// There is no existing Sales report that sums invoice totals for the current
/// calendar month specifically — every Sales invoice report/aggregation
/// (GetIssuedInvoiceTotalsBySalesPersonAsync, and the
/// GetSalesCommissionSummaryReportQuery built on top of it) is all-time,
/// scoped only to Status == Issued. Per this task's own guidance ("if none
/// exists at exactly the granularity needed, use the closest existing one —
/// e.g. total of all Issued invoices — rather than inventing new SQL"), this
/// reuses GetSalesCommissionSummaryReportQuery (the same per-salesperson
/// Issued-invoice totals the Sales Commission report already exposes) and
/// sums TotalInvoicedRevenue across every salesperson to get the company-wide
/// total, instead of adding a new month-filtered query/SQL to Sales.
/// </summary>
public sealed class SalesRevenueKpiDataSource : IKpiDataSource
{
    private readonly ISender _sender;

    public SalesRevenueKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "SALES_REVENUE";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var lines = await _sender.Send(new GetSalesCommissionSummaryReportQuery(companyId), cancellationToken);
        return lines.Sum(l => l.TotalInvoicedRevenue);
    }
}
