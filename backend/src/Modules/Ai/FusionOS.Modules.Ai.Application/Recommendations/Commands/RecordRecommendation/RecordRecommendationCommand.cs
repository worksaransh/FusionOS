using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;

/// <summary>
/// Named "Record", not "Create" — mirrors BusinessIntelligence's RecordKpiSnapshotCommand
/// naming, since this is a manual stand-in for what 12_AI_PLATFORM.md §3 describes as an
/// event-fed producer (see Recommendation's own class doc comment for why no such
/// producer exists yet).
/// </summary>
public sealed record RecordRecommendationCommand(Guid CompanyId, string Type, Guid ReferenceId, string Summary, string ModelVersion)
    : ICommand<RecommendationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.recommendation.record" };
    public string EntityType => nameof(Domain.Recommendations.Recommendation);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
