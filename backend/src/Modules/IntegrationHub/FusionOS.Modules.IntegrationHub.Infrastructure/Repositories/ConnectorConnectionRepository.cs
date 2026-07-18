using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class ConnectorConnectionRepository : IConnectorConnectionRepository
{
    private readonly IntegrationHubDbContext _context;

    public ConnectorConnectionRepository(IntegrationHubDbContext context) => _context = context;

    public Task<ConnectorConnection?> GetByIdAsync(Guid companyId, Guid connectorConnectionId, CancellationToken cancellationToken = default) =>
        _context.ConnectorConnections.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == connectorConnectionId, cancellationToken);

    public async Task AddAsync(ConnectorConnection connection, CancellationToken cancellationToken = default) =>
        await _context.ConnectorConnections.AddAsync(connection, cancellationToken);

    public async Task<IReadOnlyList<ConnectorConnection>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.ConnectorConnections
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.ConnectedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.ConnectorConnections.CountAsync(c => c.CompanyId == companyId, cancellationToken);
}
