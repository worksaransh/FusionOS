using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "total AP outstanding" KPI (Phase 2.3). Reuses Finance's existing
/// GetApAgingReportQuery (its GrandTotal) rather than re-querying the AP ledger
/// from BI — same query the Executive Dashboard's AP Aging widget already calls.
/// </summary>
public sealed class ApOutstandingKpiDataSource : IKpiDataSource
{
    private readonly ISender _sender;

    public ApOutstandingKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "AP_OUTSTANDING";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetApAgingReportQuery(companyId), cancellationToken);
        return report.GrandTotal;
    }
}
