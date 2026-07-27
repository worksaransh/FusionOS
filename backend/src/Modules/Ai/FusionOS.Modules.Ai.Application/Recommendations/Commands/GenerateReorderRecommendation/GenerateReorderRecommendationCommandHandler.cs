using FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.GenerateReorderRecommendation;

/// <summary>
/// See <see cref="GenerateReorderRecommendationCommand"/> for the "first real
/// producer" rationale. Uses <see cref="ISender"/> to call
/// <c>GetDemandForecastQuery</c> in-process — the same intra-module
/// query-composition pattern <c>GenerateKpiNarrativeReportQueryHandler</c>
/// already uses to call other queries rather than duplicating their logic.
/// </summary>
public sealed class GenerateReorderRecommendationCommandHandler : IRequestHandler<GenerateReorderRecommendationCommand, RecommendationDto?>
{
    private const int HistoryDays = 90;
    private const int ForecastDays = 30;

    /// <summary>Below this daily-quantity threshold, a "reorder" suggestion would be noise, not a genuine signal — same spirit as KpiNarrativeReportDto's 0.5% "Flat" trend band.</summary>
    private const decimal MeaningfulDailyDemandThreshold = 0.01m;

    private readonly ISender _sender;
    private readonly IRecommendationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateReorderRecommendationCommandHandler(ISender sender, IRecommendationRepository repository, IUnitOfWork unitOfWork)
    {
        _sender = sender;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecommendationDto?> Handle(GenerateReorderRecommendationCommand request, CancellationToken cancellationToken)
    {
        var forecast = await _sender.Send(new GetDemandForecastQuery(request.CompanyId, request.ProductId, HistoryDays, ForecastDays), cancellationToken);

        if (!forecast.IsForecastable)
        {
            // Same honest-gap convention as the forecast itself — no Recommendation is
            // fabricated from data that isn't there yet.
            return null;
        }

        var trend = forecast.ProjectedDailyDemandTrend ?? 0m;
        var movingAverage = forecast.MovingAverageDailyDemand ?? 0m;

        if (trend < MeaningfulDailyDemandThreshold && movingAverage < MeaningfulDailyDemandThreshold)
        {
            // Forecastable but demand is essentially zero — nothing worth reordering for.
            return null;
        }

        var summary =
            $"Projected demand for the next {ForecastDays} days averages {trend:F1}/day (linear trend) versus a {movingAverageWindowLabel(HistoryDays)}-day moving average of {movingAverage:F1}/day, " +
            $"based on {HistoryDays} days of dispatch history — consider reviewing the reorder point for this product.";

        var recommendation = Domain.Recommendations.Recommendation.Create(
            request.CompanyId, "ReorderSuggestion", request.ProductId, summary, "linear-regression-v1");

        await _repository.AddAsync(recommendation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecommendationMapper.ToDto(recommendation);

        static int movingAverageWindowLabel(int historyDays) => Math.Min(30, historyDays);
    }
}
