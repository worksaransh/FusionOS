using FusionOS.Modules.Ai.Application.Reports.Contracts;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.GetKpiDefinitionById;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Queries.ListKpiSnapshots;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Reports.Queries.GenerateKpiNarrativeReport;

/// <summary>
/// See <see cref="GenerateKpiNarrativeReportQuery"/> for the cross-module access
/// rationale. Trend is computed by comparing the average of the first half of
/// the ordered snapshot series against the second half — a simple, honest,
/// explainable method appropriate for a narrative report, not a forecasting
/// model (demand forecasting is a separate, unbuilt capability per
/// 12_AI_PLATFORM.md §4).
/// </summary>
public sealed class GenerateKpiNarrativeReportQueryHandler : IRequestHandler<GenerateKpiNarrativeReportQuery, KpiNarrativeReportDto>
{
    // ListKpiSnapshotsQuery paginates like every other List query in this codebase;
    // there is no query parameter to filter by date range yet, so a single large
    // page is fetched and filtered client-side by RecordedAt here. This assumes
    // fewer than this many snapshots exist for one KPI within the lookback window,
    // matching the same "roughly one page fits everything today" assumption most
    // other panels in this codebase make; a production version would either page
    // through all results or have BI add a date-range query parameter.
    private const int MaxSnapshotsFetched = 2000;

    private readonly ISender _sender;

    public GenerateKpiNarrativeReportQueryHandler(ISender sender) => _sender = sender;

    public async Task<KpiNarrativeReportDto> Handle(GenerateKpiNarrativeReportQuery request, CancellationToken cancellationToken)
    {
        var kpiDefinition = await _sender.Send(new GetKpiDefinitionByIdQuery(request.CompanyId, request.KpiDefinitionId), cancellationToken);

        var snapshotPage = await _sender.Send(
            new ListKpiSnapshotsQuery(request.CompanyId, request.KpiDefinitionId, 1, MaxSnapshotsFetched),
            cancellationToken);

        var cutoff = DateTimeOffset.UtcNow.AddDays(-Math.Abs(request.LookbackDays));
        var ordered = snapshotPage.Data
            .Where(s => s.RecordedAt >= cutoff)
            .OrderBy(s => s.RecordedAt)
            .ToList();

        var unit = string.IsNullOrWhiteSpace(kpiDefinition.Unit) ? "units" : kpiDefinition.Unit;

        if (ordered.Count < 2)
        {
            var only = ordered.Count == 1 ? ordered[0].Value : 0m;
            var narrativeInsufficient = ordered.Count == 0
                ? $"{kpiDefinition.Name} has no recorded snapshots in the last {request.LookbackDays} days — not enough history to compute a trend."
                : $"{kpiDefinition.Name} has only one recorded snapshot ({only:F1} {unit}) in the last {request.LookbackDays} days — at least two snapshots are needed to compute a trend.";

            return new KpiNarrativeReportDto(
                request.KpiDefinitionId,
                kpiDefinition.Name,
                narrativeInsufficient,
                Average: only,
                Min: only,
                Max: only,
                PercentChange: 0m,
                TrendDirection: "InsufficientData",
                SnapshotCount: ordered.Count);
        }

        var values = ordered.Select(s => s.Value).ToList();
        var average = values.Average();
        var minSnapshot = ordered.OrderBy(s => s.Value).First();
        var maxSnapshot = ordered.OrderByDescending(s => s.Value).First();

        var half = ordered.Count / 2;
        var firstHalfAverage = ordered.Take(half).Select(s => s.Value).Average();
        var secondHalfAverage = ordered.Skip(half).Select(s => s.Value).Average();

        decimal percentChange;
        if (firstHalfAverage == 0m)
            percentChange = secondHalfAverage == 0m ? 0m : 100m;
        else
            percentChange = (secondHalfAverage - firstHalfAverage) / Math.Abs(firstHalfAverage) * 100m;

        var trendDirection = percentChange switch
        {
            > 0.5m => "Up",
            < -0.5m => "Down",
            _ => "Flat",
        };

        var trendWord = trendDirection switch
        {
            "Up" => "up",
            "Down" => "down",
            _ => "essentially flat, changing",
        };

        var narrative =
            $"{kpiDefinition.Name} averaged {average:F1} {unit} over the last {request.LookbackDays} days, {trendWord} {Math.Abs(percentChange):F1}% comparing the first half of the period to the second half. " +
            $"The highest recorded value was {maxSnapshot.Value:F1} {unit} on {maxSnapshot.RecordedAt:d}; the lowest was {minSnapshot.Value:F1} {unit} on {minSnapshot.RecordedAt:d}.";

        return new KpiNarrativeReportDto(
            request.KpiDefinitionId,
            kpiDefinition.Name,
            narrative,
            Average: average,
            Min: minSnapshot.Value,
            Max: maxSnapshot.Value,
            PercentChange: percentChange,
            TrendDirection: trendDirection,
            SnapshotCount: ordered.Count);
    }
}
