using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.IsFeatureEnabled;

/// <summary>
/// The actual runtime evaluation entry point — what FeatureFlagsController's GET
/// .../evaluate/{key} calls, and what FeatureFlagService (the cross-module-friendly
/// wrapper, see FusionOS.BuildingBlocks.Application.Abstractions.IFeatureFlagService)
/// sends under the hood. EvaluationId is optional — supply a stable per-caller id (e.g.
/// UserId) to get graduated rollout-percentage behavior; omit it to get a plain
/// on/off read of the flag as a whole (see FeatureFlag.Evaluate).
/// </summary>
public sealed record IsFeatureEnabledQuery(Guid CompanyId, string Key, string? EvaluationId = null)
    : IQuery<bool>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.read" };
}
