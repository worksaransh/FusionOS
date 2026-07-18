using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;

public sealed record CreateIntegrationConnectorCommand(Guid CompanyId, string Code, string Name, string Provider, ConnectorCategory Category)
    : ICommand<IntegrationConnectorDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connector.create" };
    public string EntityType => nameof(Domain.IntegrationConnectors.IntegrationConnector);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
