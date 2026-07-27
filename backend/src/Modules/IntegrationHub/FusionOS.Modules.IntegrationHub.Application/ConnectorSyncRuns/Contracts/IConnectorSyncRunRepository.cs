namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;

public interface IConnectorSyncRunRepository
{
    Task<Domain.ConnectorSyncRuns.ConnectorSyncRun?> GetByIdAsync(Guid companyId, Guid connectorSyncRunId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.ConnectorSyncRuns.ConnectorSyncRun run, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.ConnectorSyncRuns.ConnectorSyncRun>> ListAsync(Guid companyId, Guid connectorConnectionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid connectorConnectionId, CancellationToken cancellationToken = default);
}
