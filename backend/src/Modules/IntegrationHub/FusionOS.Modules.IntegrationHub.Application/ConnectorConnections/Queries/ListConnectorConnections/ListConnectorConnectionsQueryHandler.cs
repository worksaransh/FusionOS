using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Queries.ListConnectorConnections;

public sealed class ListConnectorConnectionsQueryHandler : IRequestHandler<ListConnectorConnectionsQuery, PagedResult<ConnectorConnectionDto>>
{
    private readonly IConnectorConnectionRepository _repository;

    public ListConnectorConnectionsQueryHandler(IConnectorConnectionRepository repository) => _repository = repository;

    public async Task<PagedResult<ConnectorConnectionDto>> Handle(ListConnectorConnectionsQuery request, CancellationToken cancellationToken)
    {
        var connections = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = connections.Select(ConnectorConnectionMapper.ToDto).ToList();

        return new PagedResult<ConnectorConnectionDto>(dtos, request.Page, request.PageSize, total);
    }
}
