namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

public sealed record IntegrationConnectorDto(Guid Id, string Code, string Name, string Provider, string Category, bool IsActive);

/// <summary>Single place that turns an IntegrationConnector aggregate into its DTO, shared by every handler that returns one.</summary>
public static class IntegrationConnectorMapper
{
    public static IntegrationConnectorDto ToDto(Domain.IntegrationConnectors.IntegrationConnector connector) =>
        new(connector.Id, connector.Code, connector.Name, connector.Provider, connector.Category.ToString(), connector.IsActive);
}
