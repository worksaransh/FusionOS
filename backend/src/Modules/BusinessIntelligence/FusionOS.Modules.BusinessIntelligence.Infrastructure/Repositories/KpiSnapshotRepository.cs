using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Repositories;

public sealed class KpiSnapshotRepository : IKpiSnapshotRepository
{
    private readonly BusinessIntelligenceDbContext _context;

    public KpiSnapshotRepository(BusinessIntelligenceDbContext context) => _context = context;

    public async Task AddAsync(KpiSnapshot snapshot, CancellationToken cancellationToken = default) =>
        await _context.KpiSnapshots.AddAsync(snapshot, cancellationToken);

    public async Task<IReadOnlyList<KpiSnapshot>> ListAsync(Guid companyId, Guid? kpiDefinitionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, kpiDefinitionId)
            .OrderByDescending(s => s.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? kpiDefinitionId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, kpiDefinitionId).CountAsync(cancellationToken);

    private IQueryable<KpiSnapshot> Filtered(Guid companyId, Guid? kpiDefinitionId)
    {
        var query = _context.KpiSnapshots.Where(s => s.CompanyId == companyId);
        if (kpiDefinitionId.HasValue)
            query = query.Where(s => s.KpiDefinitionId == kpiDefinitionId.Value);
        return query;
    }
}
