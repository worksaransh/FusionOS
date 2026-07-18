using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Queries.ListConnectorConnections;

public sealed record ListConnectorConnectionsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ConnectorConnectionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.connection.read" };
}
