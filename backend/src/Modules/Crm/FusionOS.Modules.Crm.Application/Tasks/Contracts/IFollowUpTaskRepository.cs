namespace FusionOS.Modules.Crm.Application.Tasks.Contracts;

public interface IFollowUpTaskRepository
{
    Task<Domain.Tasks.FollowUpTask?> GetByIdAsync(Guid companyId, Guid taskId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Tasks.FollowUpTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// All filters are optional and independent: assignedToUserId narrows to one owner's
    /// worklist, (entityType, entityId) narrows to one record's follow-ups (supply both —
    /// same pairing convention as IActivityRepository), isCompleted picks done vs open, and
    /// overdueOnly further restricts to open tasks whose DueDate has already passed.
    /// </summary>
    Task<IReadOnlyList<Domain.Tasks.FollowUpTask>> ListAsync(
        Guid companyId,
        Guid? assignedToUserId,
        string? entityType,
        Guid? entityId,
        bool? isCompleted,
        bool overdueOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid companyId,
        Guid? assignedToUserId,
        string? entityType,
        Guid? entityId,
        bool? isCompleted,
        bool overdueOnly,
        CancellationToken cancellationToken = default);
}
