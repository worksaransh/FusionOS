using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.ListIntegrationConnectors;

public sealed class ListIntegrationConnectorsQueryHandler : IRequestHandler<ListIntegrationConnectorsQuery, PagedResult<IntegrationConnectorDto>>
{
    private readonly IIntegrationConnectorRepository _repository;

    public ListIntegrationConnectorsQueryHandler(IIntegrationConnectorRepository repository) => _repository = repository;

    public async Task<PagedResult<IntegrationConnectorDto>> Handle(ListIntegrationConnectorsQuery request, CancellationToken cancellationToken)
    {
        var connectors = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = connectors.Select(IntegrationConnectorMapper.ToDto).ToList();

        return new PagedResult<IntegrationConnectorDto>(dtos, request.Page, request.PageSize, total);
    }
}
