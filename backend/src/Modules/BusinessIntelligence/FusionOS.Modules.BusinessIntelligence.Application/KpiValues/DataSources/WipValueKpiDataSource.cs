using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkInProgressSummaryReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "work-in-progress value" KPI (Phase 2.3). Create a KpiDefinition with
/// Code = "WIP_VALUE" to wire it up. Reuses Manufacturing's existing
/// GetWorkInProgressSummaryReportQuery (its TotalWipValue — every Released
/// work order's currently-issued material value) rather than re-querying
/// WorkOrder/WorkOrderComponent from BI.
/// </summary>
public sealed class WipValueKpiDataSource : IKpiDataSource
{
    private readonly ISender _sender;

    public WipValueKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "WIP_VALUE";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetWorkInProgressSummaryReportQuery(companyId), cancellationToken);
        return report.TotalWipValue;
    }
}
