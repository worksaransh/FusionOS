namespace FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

public sealed record PluginListingDto(Guid Id, string Code, string Name, string Publisher, string Category, bool IsActive);

/// <summary>Single place that turns a PluginListing aggregate into its DTO, shared by every handler that returns one.</summary>
public static class PluginListingMapper
{
    public static PluginListingDto ToDto(Domain.PluginListings.PluginListing listing) =>
        new(listing.Id, listing.Code, listing.Name, listing.Publisher, listing.Category.ToString(), listing.IsActive);
}
