using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.UpdateFeatureFlag;

/// <summary>Update deliberately excludes Key — it's the immutable business key, same convention as UpdateCostCenterCommand excluding Code.</summary>
public sealed record UpdateFeatureFlagCommand(Guid CompanyId, Guid FeatureFlagId, string Name, string? Description, int RolloutPercentage)
    : ICommand<FeatureFlagDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.manage" };
    public string EntityType => nameof(Domain.FeatureFlags.FeatureFlag);
    public Guid EntityId => FeatureFlagId;
    public string Action => "Updated";
}
