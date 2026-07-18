using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.DismissRecommendation;

public sealed record DismissRecommendationCommand(Guid CompanyId, Guid RecommendationId)
    : ICommand<RecommendationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.recommendation.dismiss" };
    public string EntityType => nameof(Domain.Recommendations.Recommendation);
    public Guid EntityId => RecommendationId;
    public string Action => "Dismissed";
}
