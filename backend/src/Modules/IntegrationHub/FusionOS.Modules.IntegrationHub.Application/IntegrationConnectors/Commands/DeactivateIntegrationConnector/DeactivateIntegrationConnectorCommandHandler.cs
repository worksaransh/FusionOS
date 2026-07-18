using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.DeactivateIntegrationConnector;

public sealed class DeactivateIntegrationConnectorCommandHandler : IRequestHandler<DeactivateIntegrationConnectorCommand, IntegrationConnectorDto>
{
    private readonly IIntegrationConnectorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateIntegrationConnectorCommandHandler(IIntegrationConnectorRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IntegrationConnectorDto> Handle(DeactivateIntegrationConnectorCommand request, CancellationToken cancellationToken)
    {
        var connector = await _repository.GetByIdAsync(request.CompanyId, request.IntegrationConnectorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Integration connector '{request.IntegrationConnectorId}' was not found.");

        connector.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return IntegrationConnectorMapper.ToDto(connector);
    }
}
