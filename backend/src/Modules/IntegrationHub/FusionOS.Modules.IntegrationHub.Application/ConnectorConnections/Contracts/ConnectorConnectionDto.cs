namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

public sealed record ConnectorConnectionDto(Guid Id, Guid IntegrationConnectorId, string Label, string Status, DateTimeOffset ConnectedAt, DateTimeOffset? DisconnectedAt);

/// <summary>Single place that turns a ConnectorConnection aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ConnectorConnectionMapper
{
    public static ConnectorConnectionDto ToDto(Domain.ConnectorConnections.ConnectorConnection connection) =>
        new(connection.Id, connection.IntegrationConnectorId, connection.Label, connection.Status.ToString(), connection.ConnectedAt, connection.DisconnectedAt);
}
