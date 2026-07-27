using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Domain.InspectionPlans;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class InspectionPlanRepository : IInspectionPlanRepository
{
    private readonly QualityDbContext _context;

    public InspectionPlanRepository(QualityDbContext context) => _context = context;

    // .Include(x => x.Characteristics) is required because the collection is backed by a private field.
    public Task<InspectionPlan?> GetByIdAsync(Guid companyId, Guid inspectionPlanId, CancellationToken cancellationToken = default) =>
        _context.InspectionPlans.Include(p => p.Characteristics).FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == inspectionPlanId, cancellationToken);

    public async Task AddAsync(InspectionPlan plan, CancellationToken cancellationToken = default) =>
        await _context.InspectionPlans.AddAsync(plan, cancellationToken);

    public async Task<IReadOnlyList<InspectionPlan>> ListAsync(Guid companyId, InspectionType? inspectionType, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, inspectionType)
            .Include(p => p.Characteristics)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, InspectionType? inspectionType, CancellationToken cancellationToken = default) =>
        Filtered(companyId, inspectionType).CountAsync(cancellationToken);

    private IQueryable<InspectionPlan> Filtered(Guid companyId, InspectionType? inspectionType)
    {
        var query = _context.InspectionPlans.Where(p => p.CompanyId == companyId);
        if (inspectionType.HasValue)
            query = query.Where(p => p.InspectionType == inspectionType.Value);
        return query;
    }
}
