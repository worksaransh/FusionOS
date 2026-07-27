using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkCenters;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class WorkCenterRepository : IWorkCenterRepository
{
    private readonly ManufacturingDbContext _context;

    public WorkCenterRepository(ManufacturingDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.WorkCenters.AnyAsync(w => w.CompanyId == companyId && w.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public Task<WorkCenter?> GetByIdAsync(Guid companyId, Guid workCenterId, CancellationToken cancellationToken = default) =>
        _context.WorkCenters.FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Id == workCenterId, cancellationToken);

    public async Task AddAsync(WorkCenter workCenter, CancellationToken cancellationToken = default) =>
        await _context.WorkCenters.AddAsync(workCenter, cancellationToken);

    public async Task<IReadOnlyList<WorkCenter>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.WorkCenters
            .Where(w => w.CompanyId == companyId)
            .OrderBy(w => w.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WorkCenters.CountAsync(w => w.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<WorkCenter>> ListAllActiveAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _context.WorkCenters
            .Where(w => w.CompanyId == companyId && w.IsActive)
            .OrderBy(w => w.Code)
            .ToListAsync(cancellationToken);
}
