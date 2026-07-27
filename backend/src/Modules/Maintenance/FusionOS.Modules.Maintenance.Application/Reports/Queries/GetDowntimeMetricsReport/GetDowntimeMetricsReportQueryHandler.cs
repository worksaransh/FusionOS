using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Application.Reports.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Reports.Queries.GetDowntimeMetricsReport;

/// <summary>
/// Computes MTTR/MTBF from a single dataset: every Completed maintenance request
/// (any type) whose completion fell within the requested period
/// (IMaintenanceRequestRepository.ListCompletedInPeriodAsync), optionally scoped
/// to one asset.
///
/// MTTR, per asset, is the average ActualDowntimeMinutes across that asset's
/// completed requests in the period (any type — a Preventive request's downtime
/// counts toward "how long does a repair typically take" just as much as a
/// Breakdown's does).
///
/// MTBF, per asset, only considers the Breakdown-typed subset of that same
/// dataset, ordered by CreatedAt, and averages the gaps between consecutive
/// timestamps. This is a deliberate simplification: a more complete MTBF would
/// also look at the breakdown immediately preceding the period (to compute the
/// first in-period gap) and would count every Breakdown request regardless of
/// whether it has been completed yet. Both of those would require a second,
/// differently-filtered repository query; reusing the same in-period-completed
/// dataset keeps this handler's data access to one round trip and keeps MTTR/MTBF
/// mutually consistent (same underlying rows), at the cost of undercounting
/// breakdowns that are still open, or were completed outside the period, at
/// report time. Documented on DowntimeMetricsAssetLineDto as well, next to the
/// null-when-not-computable convention it also introduces.
/// </summary>
public sealed class GetDowntimeMetricsReportQueryHandler : IRequestHandler<GetDowntimeMetricsReportQuery, DowntimeMetricsReportDto>
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IAssetRepository _assetRepository;

    public GetDowntimeMetricsReportQueryHandler(IMaintenanceRequestRepository requestRepository, IAssetRepository assetRepository)
    {
        _requestRepository = requestRepository;
        _assetRepository = assetRepository;
    }

    public async Task<DowntimeMetricsReportDto> Handle(GetDowntimeMetricsReportQuery request, CancellationToken cancellationToken)
    {
        var completedRequests = await _requestRepository.ListCompletedInPeriodAsync(
            request.CompanyId, request.AssetId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        var lines = new List<DowntimeMetricsAssetLineDto>();

        // Batch-resolve every distinct AssetId referenced across the period's completed
        // requests in one round trip instead of one GetByIdAsync call per asset group
        // (was N+1 for periods touching many assets).
        var distinctAssetIds = completedRequests.Select(r => r.AssetId).Distinct().ToList();
        var assets = await _assetRepository.GetByIdsAsync(request.CompanyId, distinctAssetIds, cancellationToken);
        var assetById = assets.ToDictionary(a => a.Id);

        foreach (var group in completedRequests.GroupBy(r => r.AssetId))
        {
            assetById.TryGetValue(group.Key, out var asset);

            var downtimeValues = group
                .Where(r => r.ActualDowntimeMinutes.HasValue)
                .Select(r => r.ActualDowntimeMinutes!.Value)
                .ToList();

            var totalDowntime = downtimeValues.Sum();
            var mttr = downtimeValues.Count > 0 ? (decimal?)downtimeValues.Average() : null;

            var breakdowns = group
                .Where(r => r.Type == MaintenanceRequestType.Breakdown)
                .OrderBy(r => r.CreatedAt)
                .ToList();

            decimal? mtbf = null;
            if (breakdowns.Count >= 2)
            {
                var gaps = new List<double>();
                for (var i = 1; i < breakdowns.Count; i++)
                    gaps.Add((breakdowns[i].CreatedAt - breakdowns[i - 1].CreatedAt).TotalMinutes);

                mtbf = (decimal)gaps.Average();
            }

            lines.Add(new DowntimeMetricsAssetLineDto(
                group.Key,
                asset?.Code ?? "(unknown)",
                asset?.Name ?? "(unknown)",
                breakdowns.Count,
                totalDowntime,
                mttr,
                mtbf));
        }

        lines = lines.OrderBy(l => l.AssetCode).ToList();

        var allDowntimeValues = completedRequests.Where(r => r.ActualDowntimeMinutes.HasValue).Select(r => r.ActualDowntimeMinutes!.Value).ToList();
        var companyWideMttr = allDowntimeValues.Count > 0 ? (decimal?)allDowntimeValues.Average() : null;

        var computableAssetMtbfs = lines.Where(l => l.MtbfMinutes.HasValue).Select(l => l.MtbfMinutes!.Value).ToList();
        var companyWideMtbf = computableAssetMtbfs.Count > 0 ? (decimal?)computableAssetMtbfs.Average() : null;

        return new DowntimeMetricsReportDto(
            request.PeriodStart,
            request.PeriodEnd,
            lines,
            lines.Sum(l => l.BreakdownCount),
            lines.Sum(l => l.TotalDowntimeMinutes),
            companyWideMttr,
            companyWideMtbf);
    }
}
