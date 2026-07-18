using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.DisablePluginInstallation;

public sealed record DisablePluginInstallationCommand(Guid CompanyId, Guid PluginInstallationId)
    : ICommand<PluginInstallationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-installation.disable" };
    public string EntityType => nameof(Domain.PluginInstallations.PluginInstallation);
    public Guid EntityId => PluginInstallationId;
    public string Action => "Disabled";
}
