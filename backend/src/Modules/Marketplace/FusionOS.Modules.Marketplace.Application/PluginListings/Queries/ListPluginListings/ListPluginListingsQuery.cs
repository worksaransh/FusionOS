using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Queries.ListPluginListings;

public sealed record ListPluginListingsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PluginListingDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-listing.read" };
}
