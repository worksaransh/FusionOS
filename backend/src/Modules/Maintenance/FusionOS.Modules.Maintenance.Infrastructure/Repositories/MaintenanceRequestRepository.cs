using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly MaintenanceDbContext _context;

    public MaintenanceRequestRepository(MaintenanceDbContext context) => _context = context;

    public Task<MaintenanceRequest?> GetByIdAsync(Guid companyId, Guid maintenanceRequestId, CancellationToken cancellationToken = default) =>
        _context.MaintenanceRequests.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == maintenanceRequestId, cancellationToken);

    public async Task AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default) =>
        await _context.MaintenanceRequests.AddAsync(request, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceRequest>> ListAsync(Guid companyId, Guid? assetId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assetId)
            .OrderByDescending(r => r.ReportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? assetId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, assetId).CountAsync(cancellationToken);

    private IQueryable<MaintenanceRequest> Filtered(Guid companyId, Guid? assetId)
    {
        var query = _context.MaintenanceRequests.Where(r => r.CompanyId == companyId);
        if (assetId.HasValue)
            query = query.Where(r => r.AssetId == assetId.Value);
        return query;
    }
}
