using FusionOS.Modules.Crm.Application.Activities.Contracts;
using FusionOS.Modules.Crm.Domain.Activities;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly CrmDbContext _context;

    public ActivityRepository(CrmDbContext context) => _context = context;

    public Task<Activity?> GetByIdAsync(Guid companyId, Guid activityId, CancellationToken cancellationToken = default) =>
        _context.Activities.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == activityId, cancellationToken);

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken = default) =>
        await _context.Activities.AddAsync(activity, cancellationToken);

    public async Task<IReadOnlyList<Activity>> ListAsync(Guid companyId, string? entityType, Guid? entityId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, entityType, entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? entityType, Guid? entityId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, entityType, entityId).CountAsync(cancellationToken);

    private IQueryable<Activity> Filtered(Guid companyId, string? entityType, Guid? entityId)
    {
        var query = _context.Activities.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (entityId is { } id)
            query = query.Where(a => a.EntityId == id);

        return query;
    }
}
