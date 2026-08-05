using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "low-stock item count" KPI (Phase 2.3). Reuses Inventory's existing
/// GetStockValuationReportQuery — the same on-hand-quantity lines the Executive
/// Dashboard's Low-Stock widget already filters client-side against
/// LOW_STOCK_THRESHOLD — instead of querying the inventory ledger directly from BI.
/// </summary>
public sealed class LowStockItemCountKpiDataSource : IKpiDataSource
{
    /// <summary>Mirrors the frontend's LOW_STOCK_THRESHOLD constant in DashboardPage.tsx.</summary>
    public const decimal LowStockThreshold = 10m;

    private readonly ISender _sender;

    public LowStockItemCountKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "LOW_STOCK_COUNT";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetStockValuationReportQuery(companyId), cancellationToken);
        return report.Lines.Count(l => l.OnHandQuantity < LowStockThreshold);
    }
}
