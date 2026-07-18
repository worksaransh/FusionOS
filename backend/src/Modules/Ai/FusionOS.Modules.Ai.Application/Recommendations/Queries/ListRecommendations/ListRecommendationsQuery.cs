using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.Recommendations;

namespace FusionOS.Modules.Ai.Application.Recommendations.Queries.ListRecommendations;

/// <summary>Status is optional — omitted, this lists every recommendation for the company; supplied (typically Pending), it scopes to the inbox of decisions still awaiting a human.</summary>
public sealed record ListRecommendationsQuery(Guid CompanyId, RecommendationStatus? Status = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<RecommendationDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.recommendation.read" };
}
