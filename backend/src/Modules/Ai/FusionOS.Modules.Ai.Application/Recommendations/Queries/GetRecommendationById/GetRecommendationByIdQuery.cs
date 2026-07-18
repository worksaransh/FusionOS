using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;

namespace FusionOS.Modules.Ai.Application.Recommendations.Queries.GetRecommendationById;

public sealed record GetRecommendationByIdQuery(Guid CompanyId, Guid RecommendationId) : IQuery<RecommendationDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.recommendation.read" };
}
