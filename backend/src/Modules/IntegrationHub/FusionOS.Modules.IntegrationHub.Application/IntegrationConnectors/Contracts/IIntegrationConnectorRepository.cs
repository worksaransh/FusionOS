namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

public interface IIntegrationConnectorRepository
{
    Task<Domain.IntegrationConnectors.IntegrationConnector?> GetByIdAsync(Guid companyId, Guid integrationConnectorId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid integrationConnectorId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.IntegrationConnectors.IntegrationConnector connector, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.IntegrationConnectors.IntegrationConnector>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
