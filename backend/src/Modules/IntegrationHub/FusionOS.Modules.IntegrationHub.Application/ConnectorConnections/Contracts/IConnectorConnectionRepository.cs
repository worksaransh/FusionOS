namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

public interface IConnectorConnectionRepository
{
    Task<Domain.ConnectorConnections.ConnectorConnection?> GetByIdAsync(Guid companyId, Guid connectorConnectionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.ConnectorConnections.ConnectorConnection connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.ConnectorConnections.ConnectorConnection>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
