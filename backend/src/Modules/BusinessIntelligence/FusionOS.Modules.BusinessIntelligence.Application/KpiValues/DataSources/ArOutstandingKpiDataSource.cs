using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "total AR outstanding" KPI (Phase 2.3). Reuses Finance's existing
/// GetArAgingReportQuery (its GrandTotal) rather than re-querying the AR ledger
/// from BI — same query the Executive Dashboard's AR Aging widget already calls.
/// </summary>
public sealed class ArOutstandingKpiDataSource : IKpiDataSource
{
    private readonly ISender _sender;

    public ArOutstandingKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "AR_OUTSTANDING";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetArAgingReportQuery(companyId), cancellationToken);
        return report.GrandTotal;
    }
}
