using FusionOS.Modules.Crm.Application.Tasks.Contracts;
using FusionOS.Modules.Crm.Domain.Tasks;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

/// <summary>
/// NOTE for whoever wires this up: this repository assumes a `DbSet&lt;FollowUpTask&gt;
/// FollowUpTasks` property on CrmDbContext (same shape as every other DbSet there) and
/// registration as `services.AddScoped&lt;IFollowUpTaskRepository, FollowUpTaskRepository&gt;()`
/// in CrmModule.RegisterServices, plus an EF Core migration for the new
/// crm.follow_up_tasks table (FollowUpTaskConfiguration.cs already describes its shape).
/// This session was told not to touch CrmDbContext.cs / CrmModule.cs directly.
/// </summary>
public sealed class FollowUpTaskRepository : IFollowUpTaskRepository
{
    private readonly CrmDbContext _context;

    public FollowUpTaskRepository(CrmDbContext context) => _context = context;

    public Task<FollowUpTask?> GetByIdAsync(Guid companyId, Guid taskId, CancellationToken cancellationToken = default) =>
        _context.FollowUpTasks.FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == taskId, cancellationToken);

    public async Task AddAsync(FollowUpTask task, CancellationToken cancellationToken = default) =>
        await _context.FollowUpTasks.AddAsync(task, cancellationToken);

    public async Task<IReadOnlyList<FollowUpTask>> ListAsync(
        Guid companyId,
        Guid? assignedToUserId,
        string? entityType,
        Guid? entityId,
        bool? isCompleted,
        bool overdueOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assignedToUserId, entityType, entityId, isCompleted, overdueOnly)
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(
        Guid companyId,
        Guid? assignedToUserId,
        string? entityType,
        Guid? entityId,
        bool? isCompleted,
        bool overdueOnly,
        CancellationToken cancellationToken = default) =>
        Filtered(companyId, assignedToUserId, entityType, entityId, isCompleted, overdueOnly).CountAsync(cancellationToken);

    private IQueryable<FollowUpTask> Filtered(
        Guid companyId,
        Guid? assignedToUserId,
        string? entityType,
        Guid? entityId,
        bool? isCompleted,
        bool overdueOnly)
    {
        var query = _context.FollowUpTasks.Where(t => t.CompanyId == companyId);

        if (assignedToUserId is { } userId)
            query = query.Where(t => t.AssignedToUserId == userId);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(t => t.EntityType == entityType);
        if (entityId is { } id)
            query = query.Where(t => t.EntityId == id);
        if (isCompleted is { } completed)
            query = query.Where(t => t.IsCompleted == completed);
        if (overdueOnly)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(t => !t.IsCompleted && t.DueDate < now);
        }

        return query;
    }
}
