using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly MaintenanceDbContext _context;

    public MaintenanceRequestRepository(MaintenanceDbContext context) => _context = context;

    // .Include(x => x.ChecklistItems) and .Include(x => x.SparePartUsages) are both required
    // because those collections are backed by private fields with no lazy loading configured —
    // omitting either silently drops that collection on re-fetch (the SparePartUsages omission
    // previously undercounted MaintenanceRequest.TotalCost everywhere a request was re-fetched).
    public Task<MaintenanceRequest?> GetByIdAsync(Guid companyId, Guid maintenanceRequestId, CancellationToken cancellationToken = default) =>
        _context.MaintenanceRequests.Include(r => r.ChecklistItems).Include(r => r.SparePartUsages).FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == maintenanceRequestId, cancellationToken);

    public async Task AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default) =>
        await _context.MaintenanceRequests.AddAsync(request, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceRequest>> ListAsync(Guid companyId, Guid? assetId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assetId)
            .Include(r => r.ChecklistItems)
            .Include(r => r.SparePartUsages)
            .OrderByDescending(r => r.ReportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? assetId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, assetId).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<MaintenanceRequest>> ListCompletedInPeriodAsync(Guid companyId, Guid? assetId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assetId)
            .Where(r => r.Status == MaintenanceRequestStatus.Completed && r.CompletedAt != null && r.CompletedAt >= periodStart && r.CompletedAt <= periodEnd)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<MaintenanceRequest> Filtered(Guid companyId, Guid? assetId)
    {
        var query = _context.MaintenanceRequests.Where(r => r.CompanyId == companyId);
        if (assetId.HasValue)
            query = query.Where(r => r.AssetId == assetId.Value);
        return query;
    }
}
