using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "open purchase order value" KPI (Phase 2.3). Create a KpiDefinition
/// with Code = "OPEN_PO_VALUE" to wire it up.
///
/// Procurement's existing PO status summary report — GetPoStatusSummaryReportQuery,
/// the same query the Executive Dashboard's "Pending Approvals" widget calls
/// (DashboardPage.tsx's poStatusQuery) — only returns a count per
/// PurchaseOrderStatus (PoStatusSummaryReportDto.Lines is IReadOnlyList&lt;PoStatusSummaryLineDto(string
/// Status, int Count)&gt;); it carries no monetary total, so it cannot answer
/// "value". The only existing Application-layer query that exposes each
/// purchase order's TotalAmount is ListPurchaseOrdersQuery (used by the PO
/// list screen), so this data source calls that instead — with a page size
/// large enough to cover the whole company in one call — and sums TotalAmount
/// for every order whose status isn't a terminal/closed one.
///
/// "Open" here means anything still in flight: Draft, PendingApproval,
/// Approved, or PartiallyReceived. FullyReceived (fulfilled/closed),
/// Cancelled, and Rejected are excluded as terminal/non-open.
/// </summary>
public sealed class OpenPurchaseOrderValueKpiDataSource : IKpiDataSource
{
    private static readonly HashSet<string> ClosedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "FullyReceived",
        "Cancelled",
        "Rejected",
    };

    private readonly ISender _sender;

    public OpenPurchaseOrderValueKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "OPEN_PO_VALUE";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var page = await _sender.Send(new ListPurchaseOrdersQuery(companyId, Page: 1, PageSize: int.MaxValue), cancellationToken);
        return page.Data.Where(po => !ClosedStatuses.Contains(po.Status)).Sum(po => po.TotalAmount);
    }
}
