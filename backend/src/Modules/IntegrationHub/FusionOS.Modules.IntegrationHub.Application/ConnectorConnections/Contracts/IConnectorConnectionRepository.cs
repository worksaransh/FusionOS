namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

public interface IConnectorConnectionRepository
{
    Task<Domain.ConnectorConnections.ConnectorConnection?> GetByIdAsync(Guid companyId, Guid connectorConnectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Company-less lookup by id alone — used only by inbound webhook receivers
    /// (WebhooksController), which authenticate via a provider-specific HMAC signature
    /// rather than a FusionOS user/company context, so they have no CompanyId to scope
    /// the normal lookup by. The connection's own CompanyId is read off the returned
    /// aggregate once found.
    /// </summary>
    Task<Domain.ConnectorConnections.ConnectorConnection?> GetByIdAsync(Guid connectorConnectionId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.ConnectorConnections.ConnectorConnection connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.ConnectorConnections.ConnectorConnection>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
