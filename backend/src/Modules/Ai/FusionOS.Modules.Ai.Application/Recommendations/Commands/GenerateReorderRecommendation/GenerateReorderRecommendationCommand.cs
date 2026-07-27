using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.GenerateReorderRecommendation;

/// <summary>
/// The real, automated "producer" Recommendation's own class doc comment says
/// doesn't exist yet — replacing "manual-only" (RecordRecommendationCommand)
/// with one genuine pipeline: run <c>GetDemandForecastQuery</c>'s linear
/// regression over real dispatch history, and if the product is forecastable
/// with meaningfully positive demand, record a "ReorderSuggestion"
/// Recommendation whose summary embeds the actual computed numbers.
///
/// Reuses <c>ai.recommendation.record</c> — same permission as the manual
/// RecordRecommendationCommand, since both produce the same kind of durable
/// record (a Recommendation) via a different, now-automated path; no new
/// permission is warranted for what is still the same write operation.
/// </summary>
public sealed record GenerateReorderRecommendationCommand(Guid CompanyId, Guid ProductId)
    : ICommand<RecommendationDto?>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.recommendation.record" };
    public string EntityType => nameof(Domain.Recommendations.Recommendation);
    public Guid EntityId => ProductId;
    public string Action => "Created";
}
