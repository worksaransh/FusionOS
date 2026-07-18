using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginListings;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;

public sealed record CreatePluginListingCommand(Guid CompanyId, string Code, string Name, string Publisher, PluginCategory Category)
    : ICommand<PluginListingDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-listing.create" };
    public string EntityType => nameof(Domain.PluginListings.PluginListing);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
