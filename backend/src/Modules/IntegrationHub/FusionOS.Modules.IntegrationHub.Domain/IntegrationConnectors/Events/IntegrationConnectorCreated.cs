using FusionOS.SharedKernel;

namespace FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors.Events;

/// <summary>Raised on IntegrationConnector creation. No consumer this slice — same deliberate restraint as Marketplace's PluginListingCreated.</summary>
public sealed record IntegrationConnectorCreated(Guid IntegrationConnectorId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
