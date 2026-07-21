using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.ToggleFeatureFlag;

/// <summary>
/// Flips IsEnabled — a dedicated fast-path action separate from UpdateFeatureFlagCommand,
/// matching this codebase's convention of small dedicated workflow-transition commands
/// (e.g. PluginInstallation's Enable/Disable, NonConformanceReport's UpdateStatus) rather
/// than routing every field change through one generic PUT.
/// </summary>
public sealed record ToggleFeatureFlagCommand(Guid CompanyId, Guid FeatureFlagId)
    : ICommand<FeatureFlagDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.manage" };
    public string EntityType => nameof(Domain.FeatureFlags.FeatureFlag);
    public Guid EntityId => FeatureFlagId;
    public string Action => "Toggled";
}
