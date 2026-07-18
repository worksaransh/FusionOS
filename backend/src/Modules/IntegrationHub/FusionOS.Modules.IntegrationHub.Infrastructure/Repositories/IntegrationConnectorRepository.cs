using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class IntegrationConnectorRepository : IIntegrationConnectorRepository
{
    private readonly IntegrationHubDbContext _context;

    public IntegrationConnectorRepository(IntegrationHubDbContext context) => _context = context;

    public Task<IntegrationConnector?> GetByIdAsync(Guid companyId, Guid integrationConnectorId, CancellationToken cancellationToken = default) =>
        _context.IntegrationConnectors.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == integrationConnectorId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid integrationConnectorId, CancellationToken cancellationToken = default) =>
        _context.IntegrationConnectors.AnyAsync(c => c.CompanyId == companyId && c.Id == integrationConnectorId, cancellationToken);

    public async Task AddAsync(IntegrationConnector connector, CancellationToken cancellationToken = default) =>
        await _context.IntegrationConnectors.AddAsync(connector, cancellationToken);

    public async Task<IReadOnlyList<IntegrationConnector>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<IntegrationConnector> Filtered(Guid companyId, string? search)
    {
        var query = _context.IntegrationConnectors.Where(c => c.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Code, pattern) || EF.Functions.ILike(c.Name, pattern));
        }
        return query;
    }
}
