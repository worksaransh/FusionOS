using FusionOS.Modules.Ai.Application.DemandHistory.Contracts;
using FusionOS.Modules.Ai.Application.Forecasting.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;

/// <summary>
/// Computes a genuinely calculated, deliberately simple demand forecast for one
/// product from its real <c>DemandHistoryEntry</c> rows (fed by Sales'
/// DispatchLineDispatched integration event — see DemandHistoryEntry's own
/// doc comment). This is the honest, available-today alternative to the
/// Python time-series ML service 12_AI_PLATFORM.md §3 describes as ideal but
/// out of reach in this environment — not a fabricated placeholder.
///
/// Method:
/// 1. Build a complete, gap-filled daily series over [today - HistoryDays + 1, today]:
///    every calendar day in the window gets one point, summing that day's
///    DemandHistoryEntry.QuantityDispatched (0 if no dispatch occurred that day).
///    Gap-filling (rather than only using days with data) is what makes the
///    regression/average genuinely reflect "demand per day", including the
///    zero-dispatch days a real demand series has.
/// 2. "Insufficient history" is judged on the number of DISTINCT calendar days
///    that had at least one real dispatch recorded — not the gap-filled series
///    length — since a long-but-nearly-all-zero window filled mostly with
///    zero-fill days out of {HistoryDays} would understate a genuine data gap.
///    Fewer than 7 such days and no number is fabricated: IsForecastable=false,
///    same "null/honest-gap when data is insufficient" convention as
///    Maintenance's MTBF calculation and this module's own
///    KpiNarrativeReportDto ("InsufficientData" trend).
/// 3. Otherwise, fit a simple linear least-squares line (LeastSquaresRegression,
///    day-index vs. daily quantity) over the gap-filled series and project it
///    ForecastDays forward, clamped at zero (demand can't be negative) — this
///    is ProjectedDailyDemandTrend/ProjectedPoints. A second, more conservative
///    figure — MovingAverageDailyDemand — is the plain average of the trailing
///    min(30, HistoryDays) days of the same gap-filled series, so a caller can
///    compare a trend-reactive number against a flatter one.
///
/// Deliberately NOT a real ML model: ModelVersion "linear-regression-v1" is
/// what a real producer (GenerateReorderRecommendationCommand) records when it
/// turns this calculation into a Recommendation.
/// </summary>
public sealed class GetDemandForecastQueryHandler : IRequestHandler<GetDemandForecastQuery, DemandForecastDto>
{
    private const int MinimumDistinctDaysRequired = 7;
    private const int MovingAverageWindowDays = 30;

    private readonly IDemandHistoryRepository _repository;

    public GetDemandForecastQueryHandler(IDemandHistoryRepository repository) => _repository = repository;

    public async Task<DemandForecastDto> Handle(GetDemandForecastQuery request, CancellationToken cancellationToken)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var periodStart = today.AddDays(-(request.HistoryDays - 1));
        var periodEnd = today.AddDays(1).AddTicks(-1); // end of today, inclusive

        var entries = await _repository.ListForProductAsync(
            request.CompanyId, request.ProductId,
            new DateTimeOffset(periodStart, TimeSpan.Zero), new DateTimeOffset(periodEnd, TimeSpan.Zero),
            cancellationToken);

        var byDay = entries
            .GroupBy(e => e.PeriodDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.QuantityDispatched));

        if (byDay.Count < MinimumDistinctDaysRequired)
        {
            return new DemandForecastDto(
                request.ProductId,
                IsForecastable: false,
                InsufficientDataNote: $"Insufficient history — only {byDay.Count} distinct day(s) with recorded dispatches in the last {request.HistoryDays} days; needs at least {MinimumDistinctDaysRequired} days of dispatch data.",
                ProjectedDailyDemandTrend: null,
                MovingAverageDailyDemand: null,
                ProjectedPoints: Array.Empty<ForecastPointDto>());
        }

        // Gap-filled daily series: every day in the window gets a point (0 if no dispatch that day).
        var dailySeries = new List<(int DayIndex, decimal Quantity)>();
        for (var i = 0; i < request.HistoryDays; i++)
        {
            var day = periodStart.AddDays(i);
            dailySeries.Add((i, byDay.TryGetValue(day, out var qty) ? qty : 0m));
        }

        var (slope, intercept) = LeastSquaresRegression.Fit(dailySeries);

        var movingAverageWindow = Math.Min(MovingAverageWindowDays, request.HistoryDays);
        var movingAverage = dailySeries.TakeLast(movingAverageWindow).Average(p => p.Quantity);

        var projectedPoints = new List<ForecastPointDto>(request.ForecastDays);
        for (var i = 0; i < request.ForecastDays; i++)
        {
            var dayIndex = request.HistoryDays + i;
            var projected = Math.Max(0m, slope * dayIndex + intercept);
            projectedPoints.Add(new ForecastPointDto(today.AddDays(i + 1), projected));
        }

        // ProjectedDailyDemandTrend is the average of the projected points — a single
        // representative daily-rate figure over the forecast horizon, comparable
        // apples-to-apples against MovingAverageDailyDemand (both "quantity/day").
        var projectedDailyDemandTrend = projectedPoints.Average(p => p.ProjectedQuantity);

        return new DemandForecastDto(
            request.ProductId,
            IsForecastable: true,
            InsufficientDataNote: null,
            ProjectedDailyDemandTrend: projectedDailyDemandTrend,
            MovingAverageDailyDemand: movingAverage,
            ProjectedPoints: projectedPoints);
    }
}
