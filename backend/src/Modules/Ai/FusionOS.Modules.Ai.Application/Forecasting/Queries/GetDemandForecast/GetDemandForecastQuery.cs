using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Forecasting.Contracts;

namespace FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;

/// <summary>
/// Uses <c>ai.forecast.read</c> rather than <c>ai.forecast.generate</c> — this
/// query only reads already-recorded DemandHistoryEntry rows and performs a
/// pure, side-effect-free calculation over them (no write, nothing recorded);
/// "generate" is reserved for the action that actually produces and persists
/// a durable artifact, i.e. <c>GenerateReorderRecommendationCommand</c> below,
/// which is what turns a forecast into a recorded Recommendation.
/// </summary>
public sealed record GetDemandForecastQuery(Guid CompanyId, Guid ProductId, int HistoryDays = 90, int ForecastDays = 30)
    : IQuery<DemandForecastDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.forecast.read" };
}
