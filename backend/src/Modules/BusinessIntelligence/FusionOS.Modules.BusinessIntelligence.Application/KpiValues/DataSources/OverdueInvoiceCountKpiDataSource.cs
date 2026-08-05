using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.DataSources;

/// <summary>
/// Live "overdue invoice count" KPI (Phase 2.3). Create a KpiDefinition with
/// Code = "AR_OVERDUE_COUNT" to wire it up.
///
/// Calls the exact same Finance query ArOutstandingKpiDataSource already
/// calls — GetArAgingReportQuery(companyId) — but instead of summing
/// GrandTotal, this counts lines whose Bucket is not "0-30" (the current,
/// not-yet-overdue period; GetArAgingReportQueryHandler buckets as "0-30",
/// "31-60", "61-90", "90+"), i.e. every outstanding invoice balance that has
/// aged past the current period.
/// </summary>
public sealed class OverdueInvoiceCountKpiDataSource : IKpiDataSource
{
    private const string CurrentPeriodBucket = "0-30";

    private readonly ISender _sender;

    public OverdueInvoiceCountKpiDataSource(ISender sender) => _sender = sender;

    public string KpiCode => "AR_OVERDUE_COUNT";

    public async Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetArAgingReportQuery(companyId), cancellationToken);
        return report.Lines.Count(l => l.Bucket != CurrentPeriodBucket);
    }
}
