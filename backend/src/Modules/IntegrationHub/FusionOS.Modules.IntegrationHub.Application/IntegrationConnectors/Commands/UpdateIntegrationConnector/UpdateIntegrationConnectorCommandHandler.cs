using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.UpdateIntegrationConnector;

public sealed class UpdateIntegrationConnectorCommandHandler : IRequestHandler<UpdateIntegrationConnectorCommand, IntegrationConnectorDto>
{
    private readonly IIntegrationConnectorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIntegrationConnectorCommandHandler(IIntegrationConnectorRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IntegrationConnectorDto> Handle(UpdateIntegrationConnectorCommand request, CancellationToken cancellationToken)
    {
        var connector = await _repository.GetByIdAsync(request.CompanyId, request.IntegrationConnectorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Integration connector '{request.IntegrationConnectorId}' was not found.");

        connector.UpdateDetails(request.Name, request.Provider, request.Category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return IntegrationConnectorMapper.ToDto(connector);
    }
}
