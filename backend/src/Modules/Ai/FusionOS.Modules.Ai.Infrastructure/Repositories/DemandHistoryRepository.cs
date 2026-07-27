using FusionOS.Modules.Ai.Application.DemandHistory.Contracts;
using FusionOS.Modules.Ai.Domain.DemandHistory;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Repositories;

public sealed class DemandHistoryRepository : IDemandHistoryRepository
{
    private readonly AiDbContext _context;

    public DemandHistoryRepository(AiDbContext context) => _context = context;

    public async Task AddAsync(DemandHistoryEntry entry, CancellationToken cancellationToken = default) =>
        await _context.DemandHistoryEntries.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<DemandHistoryEntry>> ListForProductAsync(Guid companyId, Guid productId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default) =>
        await _context.DemandHistoryEntries
            .Where(e => e.CompanyId == companyId && e.ProductId == productId && e.PeriodDate >= periodStart && e.PeriodDate <= periodEnd)
            .OrderBy(e => e.PeriodDate)
            .ToListAsync(cancellationToken);
}
