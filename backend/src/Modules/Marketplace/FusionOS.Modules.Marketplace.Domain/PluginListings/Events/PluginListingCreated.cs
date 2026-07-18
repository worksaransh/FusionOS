using FusionOS.SharedKernel;

namespace FusionOS.Modules.Marketplace.Domain.PluginListings.Events;

/// <summary>Raised on PluginListing creation. No consumer this slice — same deliberate restraint as Maintenance's AssetCreated.</summary>
public sealed record PluginListingCreated(Guid PluginListingId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
