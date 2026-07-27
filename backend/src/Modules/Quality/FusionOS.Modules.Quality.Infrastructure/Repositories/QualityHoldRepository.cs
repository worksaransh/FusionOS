using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using FusionOS.Modules.Quality.Domain.QualityHolds;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class QualityHoldRepository : IQualityHoldRepository
{
    private readonly QualityDbContext _context;

    public QualityHoldRepository(QualityDbContext context) => _context = context;

    public Task<QualityHold?> GetByIdAsync(Guid companyId, Guid qualityHoldId, CancellationToken cancellationToken = default) =>
        _context.QualityHolds.FirstOrDefaultAsync(h => h.CompanyId == companyId && h.Id == qualityHoldId, cancellationToken);

    public async Task AddAsync(QualityHold hold, CancellationToken cancellationToken = default) =>
        await _context.QualityHolds.AddAsync(hold, cancellationToken);

    public async Task<IReadOnlyList<QualityHold>> ListAsync(Guid companyId, string? entityType, Guid? entityId, bool? activeOnly, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, entityType, entityId, activeOnly)
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? entityType, Guid? entityId, bool? activeOnly, CancellationToken cancellationToken = default) =>
        Filtered(companyId, entityType, entityId, activeOnly).CountAsync(cancellationToken);

    private IQueryable<QualityHold> Filtered(Guid companyId, string? entityType, Guid? entityId, bool? activeOnly)
    {
        var query = _context.QualityHolds.Where(h => h.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(h => h.EntityType == entityType);
        if (entityId.HasValue)
            query = query.Where(h => h.EntityId == entityId.Value);
        if (activeOnly == true)
            query = query.Where(h => h.Status == QualityHoldStatus.Active);
        return query;
    }
}
