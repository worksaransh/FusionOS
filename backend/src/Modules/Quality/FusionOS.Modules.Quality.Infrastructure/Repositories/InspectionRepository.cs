using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class InspectionRepository : IInspectionRepository
{
    private readonly QualityDbContext _context;

    public InspectionRepository(QualityDbContext context) => _context = context;

    // .Include(x => x.Items) is required because the Items collection is backed by a private field.
    public Task<Inspection?> GetByIdAsync(Guid companyId, Guid inspectionId, CancellationToken cancellationToken = default) =>
        _context.Inspections.Include(i => i.Items).FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == inspectionId, cancellationToken);

    public async Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default) =>
        await _context.Inspections.AddAsync(inspection, cancellationToken);

    public async Task<IReadOnlyList<Inspection>> ListAsync(Guid companyId, InspectionType? type, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, type)
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, InspectionType? type, CancellationToken cancellationToken = default) =>
        Filtered(companyId, type).CountAsync(cancellationToken);

    private IQueryable<Inspection> Filtered(Guid companyId, InspectionType? type)
    {
        var query = _context.Inspections.Where(i => i.CompanyId == companyId);
        if (type.HasValue)
            query = query.Where(i => i.Type == type.Value);
        return query;
    }

    public async Task<IReadOnlyList<Inspection>> ListByCharacteristicAsync(
        Guid companyId, string characteristic, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, CancellationToken cancellationToken = default)
    {
        var query = _context.Inspections
            .Include(i => i.Items)
            .Where(i => i.CompanyId == companyId && i.Items.Any(item => item.Characteristic.ToLower() == characteristic.ToLower() && item.MeasuredValue != null));

        if (periodStart.HasValue)
            query = query.Where(i => i.CreatedAt >= periodStart.Value);
        if (periodEnd.HasValue)
            query = query.Where(i => i.CreatedAt <= periodEnd.Value);

        return await query.OrderBy(i => i.CreatedAt).ToListAsync(cancellationToken);
    }
}
