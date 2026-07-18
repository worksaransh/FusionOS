using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.GetIntegrationConnectorById;

public sealed record GetIntegrationConnectorByIdQuery(Guid CompanyId, Guid IntegrationConnectorId) : IQuery<IntegrationConnectorDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.connector.read" };
}
