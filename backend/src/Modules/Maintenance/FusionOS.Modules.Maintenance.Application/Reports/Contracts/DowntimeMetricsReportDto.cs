namespace FusionOS.Modules.Maintenance.Application.Reports.Contracts;

/// <summary>
/// One asset's downtime metrics for the requested period.
///
/// MttrMinutes (Mean Time To Repair) = average ActualDowntimeMinutes across every
/// Completed maintenance request (Preventive or Breakdown) for this asset whose
/// completion fell within the period. Null — not 0 — when the asset has zero such
/// requests in the period: a fabricated 0 would misleadingly claim "no downtime
/// was needed", when really there's just no data point to average. Same
/// "null when not computable, not a fabricated value" convention as Finance's
/// TaxPeriodSummaryReportDto.
///
/// MtbfMinutes (Mean Time Between Failures) = average gap, in minutes, between
/// consecutive Breakdown-type requests' CreatedAt timestamps for this asset.
/// Simplification (documented here, and in GetDowntimeMetricsReportQueryHandler):
/// this is computed only over Breakdown requests that were themselves Completed
/// within the period — the same underlying dataset MTTR uses — rather than a
/// separate "all breakdowns created in the period regardless of completion"
/// query. This keeps both metrics consistent with one data pull, at the cost of
/// undercounting a breakdown that's still open (or was completed outside the
/// period) at report time. Requires at least 2 such requests to produce a gap;
/// null otherwise, since one (or zero) data points give no interval to measure.
///
/// BreakdownCount and TotalDowntimeMinutes are simple supporting figures and are
/// always computable — 0 is a genuinely accurate value for "no breakdowns" / "no
/// downtime", not a stand-in for missing data, unlike MttrMinutes/MtbfMinutes.
/// </summary>
public sealed record DowntimeMetricsAssetLineDto(
    Guid AssetId,
    string AssetCode,
    string AssetName,
    int BreakdownCount,
    int TotalDowntimeMinutes,
    decimal? MttrMinutes,
    decimal? MtbfMinutes);

/// <summary>
/// Company-wide (or, when AssetId is supplied to the query, single-asset)
/// downtime/MTBF/MTTR summary for a period (05_MODULE_ROADMAP.md's Maintenance
/// metrics line item).
///
/// CompanyWideMttrMinutes is a straight average of ActualDowntimeMinutes across
/// every Completed request in the period, regardless of asset — null when there
/// are none.
///
/// CompanyWideMtbfMinutes is a simple mean of the per-asset MtbfMinutes values
/// that were themselves computable (non-null) — not a single gap-analysis across
/// assets, since interleaving different machines' breakdown timestamps into one
/// sequence would not mean anything ("time between asset A's breakdown and
/// unrelated asset B's breakdown" isn't a reliability signal). Null when no asset
/// has a computable MTBF in the period.
/// </summary>
public sealed record DowntimeMetricsReportDto(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    IReadOnlyList<DowntimeMetricsAssetLineDto> Lines,
    int TotalBreakdownCount,
    int TotalDowntimeMinutes,
    decimal? CompanyWideMttrMinutes,
    decimal? CompanyWideMtbfMinutes);
