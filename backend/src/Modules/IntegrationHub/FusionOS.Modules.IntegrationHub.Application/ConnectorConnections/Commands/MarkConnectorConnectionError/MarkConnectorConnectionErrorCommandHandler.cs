using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.MarkConnectorConnectionError;

public sealed class MarkConnectorConnectionErrorCommandHandler : IRequestHandler<MarkConnectorConnectionErrorCommand, ConnectorConnectionDto>
{
    private readonly IConnectorConnectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkConnectorConnectionErrorCommandHandler(IConnectorConnectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectorConnectionDto> Handle(MarkConnectorConnectionErrorCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.CompanyId, request.ConnectorConnectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Connector connection '{request.ConnectorConnectionId}' was not found.");

        connection.MarkError();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ConnectorConnectionMapper.ToDto(connection);
    }
}
