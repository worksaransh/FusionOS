using FusionOS.SharedKernel;

namespace FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections.Events;

/// <summary>Raised on connection creation. No consumer this slice — the natural future hook once a real sync engine exists to start pulling/pushing data through this connection.</summary>
public sealed record ConnectorConnected(Guid ConnectorConnectionId, Guid CompanyId, Guid IntegrationConnectorId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
