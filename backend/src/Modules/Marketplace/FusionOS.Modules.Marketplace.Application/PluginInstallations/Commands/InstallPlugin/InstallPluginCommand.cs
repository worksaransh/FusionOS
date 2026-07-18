using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.InstallPlugin;

public sealed record InstallPluginCommand(Guid CompanyId, Guid PluginListingId)
    : ICommand<PluginInstallationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-installation.install" };
    public string EntityType => nameof(Domain.PluginInstallations.PluginInstallation);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
