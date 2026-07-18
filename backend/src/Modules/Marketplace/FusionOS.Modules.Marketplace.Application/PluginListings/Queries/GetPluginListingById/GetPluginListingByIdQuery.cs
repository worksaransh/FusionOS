using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Queries.GetPluginListingById;

public sealed record GetPluginListingByIdQuery(Guid CompanyId, Guid PluginListingId) : IQuery<PluginListingDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-listing.read" };
}
