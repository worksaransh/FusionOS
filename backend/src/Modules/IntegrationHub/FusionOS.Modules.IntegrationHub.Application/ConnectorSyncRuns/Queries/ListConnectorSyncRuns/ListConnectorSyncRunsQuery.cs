using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Queries.ListConnectorSyncRuns;

public sealed record ListConnectorSyncRunsQuery(Guid CompanyId, Guid ConnectionId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ConnectorSyncRunDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.sync.read" };
}
