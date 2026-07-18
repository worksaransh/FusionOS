using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.DeactivateIntegrationConnector;

public sealed record DeactivateIntegrationConnectorCommand(Guid CompanyId, Guid IntegrationConnectorId)
    : ICommand<IntegrationConnectorDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connector.deactivate" };
    public string EntityType => nameof(Domain.IntegrationConnectors.IntegrationConnector);
    public Guid EntityId => IntegrationConnectorId;
    public string Action => "Deactivated";
}
