using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.ListIntegrationConnectors;

public sealed record ListIntegrationConnectorsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<IntegrationConnectorDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.connector.read" };
}
