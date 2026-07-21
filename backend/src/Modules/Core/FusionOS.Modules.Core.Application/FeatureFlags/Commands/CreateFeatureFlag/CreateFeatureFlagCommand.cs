using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.CreateFeatureFlag;

public sealed record CreateFeatureFlagCommand(Guid CompanyId, string Key, string Name, string? Description, int RolloutPercentage = 100)
    : ICommand<FeatureFlagDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.manage" };
    public string EntityType => nameof(Domain.FeatureFlags.FeatureFlag);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
