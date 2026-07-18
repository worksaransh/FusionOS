using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;

/// <summary>Validates the IntegrationConnector exists for this company before connecting to it — same handler-level existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.</summary>
public sealed class ConnectConnectorCommandHandler : IRequestHandler<ConnectConnectorCommand, ConnectorConnectionDto>
{
    private readonly IConnectorConnectionRepository _repository;
    private readonly IIntegrationConnectorRepository _integrationConnectorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectConnectorCommandHandler(IConnectorConnectionRepository repository, IIntegrationConnectorRepository integrationConnectorRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _integrationConnectorRepository = integrationConnectorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectorConnectionDto> Handle(ConnectConnectorCommand request, CancellationToken cancellationToken)
    {
        if (!await _integrationConnectorRepository.ExistsAsync(request.CompanyId, request.IntegrationConnectorId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.IntegrationConnectorId), $"Integration connector '{request.IntegrationConnectorId}' does not exist for this company."),
            });
        }

        var connection = Domain.ConnectorConnections.ConnectorConnection.Create(request.CompanyId, request.IntegrationConnectorId, request.Label);

        await _repository.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ConnectorConnectionMapper.ToDto(connection);
    }
}
