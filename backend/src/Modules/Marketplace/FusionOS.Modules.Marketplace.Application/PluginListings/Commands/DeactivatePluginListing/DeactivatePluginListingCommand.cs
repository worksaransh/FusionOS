using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.DeactivatePluginListing;

public sealed record DeactivatePluginListingCommand(Guid CompanyId, Guid PluginListingId)
    : ICommand<PluginListingDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-listing.deactivate" };
    public string EntityType => nameof(Domain.PluginListings.PluginListing);
    public Guid EntityId => PluginListingId;
    public string Action => "Deactivated";
}
