namespace FusionOS.Modules.Ai.Application.Forecasting.Contracts;

/// <summary>
/// The real, honest demand forecast this pass builds in place of the Python
/// time-series ML service `docs/blueprint/12_AI_PLATFORM.md` §3 describes as
/// ideal-but-out-of-reach (no Python runtime, no training pipeline in this
/// environment). Computed entirely from <c>DemandHistoryEntry</c> rows fed by
/// real dispatched-quantity events — never fabricated.
///
/// When <see cref="IsForecastable"/> is false, <see cref="InsufficientDataNote"/>
/// explains why and every numeric field is null — the same "null/honest-gap
/// when data is insufficient" convention as Maintenance's MTBF calculation and
/// the AI module's own KpiNarrativeReportDto ("InsufficientData" trend).
///
/// Two figures are returned, deliberately not just one:
/// <see cref="ProjectedDailyDemandTrend"/> is the slope-projected linear
/// regression figure (reacts to a trend, more volatile); <see cref="MovingAverageDailyDemand"/>
/// is the flatter, more conservative simple average of the same window — a
/// caller can compare the two rather than trust a single number blindly.
/// </summary>
public sealed record DemandForecastDto(
    Guid ProductId,
    bool IsForecastable,
    string? InsufficientDataNote,
    decimal? ProjectedDailyDemandTrend,
    decimal? MovingAverageDailyDemand,
    IReadOnlyList<ForecastPointDto> ProjectedPoints);

/// <summary>One projected day: the linear-regression trend line evaluated at that day, clamped at zero (demand can't be negative).</summary>
public sealed record ForecastPointDto(DateTimeOffset Date, decimal ProjectedQuantity);
