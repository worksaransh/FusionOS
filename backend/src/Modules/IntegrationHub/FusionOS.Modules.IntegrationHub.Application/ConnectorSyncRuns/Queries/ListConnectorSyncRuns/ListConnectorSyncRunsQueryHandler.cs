using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Queries.ListConnectorSyncRuns;

public sealed class ListConnectorSyncRunsQueryHandler : IRequestHandler<ListConnectorSyncRunsQuery, PagedResult<ConnectorSyncRunDto>>
{
    private readonly IConnectorSyncRunRepository _repository;

    public ListConnectorSyncRunsQueryHandler(IConnectorSyncRunRepository repository) => _repository = repository;

    public async Task<PagedResult<ConnectorSyncRunDto>> Handle(ListConnectorSyncRunsQuery request, CancellationToken cancellationToken)
    {
        var runs = await _repository.ListAsync(request.CompanyId, request.ConnectionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ConnectionId, cancellationToken);

        var dtos = runs.Select(ConnectorSyncRunMapper.ToDto).ToList();

        return new PagedResult<ConnectorSyncRunDto>(dtos, request.Page, request.PageSize, total);
    }
}
