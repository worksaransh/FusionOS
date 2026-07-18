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

    public async Task<IReadOnlyList<Inspection>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Inspections
            .Include(i => i.Items)
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Inspections.CountAsync(i => i.CompanyId == companyId, cancellationToken);
}
