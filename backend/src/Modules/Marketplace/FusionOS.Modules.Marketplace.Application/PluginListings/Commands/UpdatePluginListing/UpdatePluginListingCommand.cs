using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginListings;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.UpdatePluginListing;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand/UpdateCompanyCommand.</summary>
public sealed record UpdatePluginListingCommand(Guid CompanyId, Guid PluginListingId, string Name, string Publisher, PluginCategory Category)
    : ICommand<PluginListingDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-listing.update" };
    public string EntityType => nameof(Domain.PluginListings.PluginListing);
    public Guid EntityId => PluginListingId;
    public string Action => "Updated";
}
