using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class MaintenanceScheduleRepository : IMaintenanceScheduleRepository
{
    /// <summary>How far out "due soon" looks from today — a schedule whose NextDueDate falls within this window (and hasn't already passed, which makes it Overdue instead) shows up in the DueSoon view.</summary>
    private static readonly TimeSpan DueSoonWindow = TimeSpan.FromDays(7);

    private readonly MaintenanceDbContext _context;

    public MaintenanceScheduleRepository(MaintenanceDbContext context) => _context = context;

    public Task<MaintenanceSchedule?> GetByIdAsync(Guid companyId, Guid maintenanceScheduleId, CancellationToken cancellationToken = default) =>
        _context.MaintenanceSchedules.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == maintenanceScheduleId, cancellationToken);

    public async Task AddAsync(MaintenanceSchedule schedule, CancellationToken cancellationToken = default) =>
        await _context.MaintenanceSchedules.AddAsync(schedule, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceSchedule>> ListAsync(Guid companyId, Guid? assetId, MaintenanceScheduleDueFilter? dueFilter, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assetId, dueFilter)
            .OrderBy(s => s.NextDueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? assetId, MaintenanceScheduleDueFilter? dueFilter, CancellationToken cancellationToken = default) =>
        Filtered(companyId, assetId, dueFilter).CountAsync(cancellationToken);

    private IQueryable<MaintenanceSchedule> Filtered(Guid companyId, Guid? assetId, MaintenanceScheduleDueFilter? dueFilter)
    {
        var query = _context.MaintenanceSchedules.Where(s => s.CompanyId == companyId);
        if (assetId.HasValue)
            query = query.Where(s => s.AssetId == assetId.Value);

        if (dueFilter is MaintenanceScheduleDueFilter.Overdue)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(s => s.IsActive && s.NextDueDate < now);
        }
        else if (dueFilter is MaintenanceScheduleDueFilter.DueSoon)
        {
            var now = DateTimeOffset.UtcNow;
            var window = now.Add(DueSoonWindow);
            query = query.Where(s => s.IsActive && s.NextDueDate >= now && s.NextDueDate <= window);
        }

        return query;
    }
}
