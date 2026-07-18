using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.GetIntegrationConnectorById;

public sealed class GetIntegrationConnectorByIdQueryHandler : IRequestHandler<GetIntegrationConnectorByIdQuery, IntegrationConnectorDto>
{
    private readonly IIntegrationConnectorRepository _repository;

    public GetIntegrationConnectorByIdQueryHandler(IIntegrationConnectorRepository repository) => _repository = repository;

    public async Task<IntegrationConnectorDto> Handle(GetIntegrationConnectorByIdQuery request, CancellationToken cancellationToken)
    {
        var connector = await _repository.GetByIdAsync(request.CompanyId, request.IntegrationConnectorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Integration connector '{request.IntegrationConnectorId}' was not found.");

        return IntegrationConnectorMapper.ToDto(connector);
    }
}
