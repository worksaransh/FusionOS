using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class ConnectorSyncRunRepository : IConnectorSyncRunRepository
{
    private readonly IntegrationHubDbContext _context;

    public ConnectorSyncRunRepository(IntegrationHubDbContext context) => _context = context;

    public Task<ConnectorSyncRun?> GetByIdAsync(Guid companyId, Guid connectorSyncRunId, CancellationToken cancellationToken = default) =>
        _context.ConnectorSyncRuns.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == connectorSyncRunId, cancellationToken);

    public async Task AddAsync(ConnectorSyncRun run, CancellationToken cancellationToken = default) =>
        await _context.ConnectorSyncRuns.AddAsync(run, cancellationToken);

    public async Task<IReadOnlyList<ConnectorSyncRun>> ListAsync(Guid companyId, Guid connectorConnectionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.ConnectorSyncRuns
            .Where(r => r.CompanyId == companyId && r.ConnectorConnectionId == connectorConnectionId)
            .OrderByDescending(r => r.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid connectorConnectionId, CancellationToken cancellationToken = default) =>
        _context.ConnectorSyncRuns.CountAsync(r => r.CompanyId == companyId && r.ConnectorConnectionId == connectorConnectionId, cancellationToken);
}
