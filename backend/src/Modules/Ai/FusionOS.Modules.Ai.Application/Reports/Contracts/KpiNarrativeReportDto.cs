namespace FusionOS.Modules.Ai.Application.Reports.Contracts;

/// <summary>
/// A narrative report generated from an existing KPI's own snapshot history
/// (12_AI_PLATFORM.md §4 "Automated report generation" — "building on Business
/// Intelligence... rather than maintaining a parallel reporting pipeline").
/// Every numeric field here is genuinely computed from the KPI's real
/// <c>KpiSnapshot</c> values over the requested lookback window — nothing is a
/// canned template value. <see cref="TrendDirection"/> is one of "Up", "Down",
/// "Flat", or "InsufficientData" (see <see cref="SnapshotCount"/>).
/// </summary>
public sealed record KpiNarrativeReportDto(
    Guid KpiDefinitionId,
    string KpiName,
    string Narrative,
    decimal Average,
    decimal Min,
    decimal Max,
    decimal PercentChange,
    string TrendDirection,
    int SnapshotCount);
