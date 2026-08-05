using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.UpdateIntegrationConnector;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand/UpdateCompanyCommand.</summary>
public sealed record UpdateIntegrationConnectorCommand(Guid CompanyId, Guid IntegrationConnectorId, string Name, string Provider, ConnectorCategory Category)
    : ICommand<IntegrationConnectorDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connector.update" };
    public string EntityType => nameof(Domain.IntegrationConnectors.IntegrationConnector);
    public Guid EntityId => IntegrationConnectorId;
    public string Action => "Updated";
}
