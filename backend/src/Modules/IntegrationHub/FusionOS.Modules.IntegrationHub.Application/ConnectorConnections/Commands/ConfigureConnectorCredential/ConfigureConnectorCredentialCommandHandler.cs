using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConfigureConnectorCredential;

public sealed class ConfigureConnectorCredentialCommandHandler : IRequestHandler<ConfigureConnectorCredentialCommand, ConnectorConnectionDto>
{
    private readonly IConnectorConnectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureConnectorCredentialCommandHandler(IConnectorConnectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectorConnectionDto> Handle(ConfigureConnectorCredentialCommand request, CancellationToken cancellationToken)
    {
        var connection = await _repository.GetByIdAsync(request.CompanyId, request.ConnectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Connector connection '{request.ConnectionId}' was not found.");

        connection.ConfigureCredential(request.CredentialReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ConnectorConnectionMapper.ToDto(connection);
    }
}
