using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;

public sealed class CreateIntegrationConnectorCommandHandler : IRequestHandler<CreateIntegrationConnectorCommand, IntegrationConnectorDto>
{
    private readonly IIntegrationConnectorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIntegrationConnectorCommandHandler(IIntegrationConnectorRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IntegrationConnectorDto> Handle(CreateIntegrationConnectorCommand request, CancellationToken cancellationToken)
    {
        var connector = Domain.IntegrationConnectors.IntegrationConnector.Create(request.CompanyId, request.Code, request.Name, request.Provider, request.Category);

        await _repository.AddAsync(connector, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return IntegrationConnectorMapper.ToDto(connector);
    }
}
