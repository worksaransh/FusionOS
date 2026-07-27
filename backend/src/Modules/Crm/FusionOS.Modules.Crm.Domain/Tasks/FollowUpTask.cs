using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Tasks.Events;

namespace FusionOS.Modules.Crm.Domain.Tasks;

/// <summary>
/// Phase 4 — CRM. A follow-up reminder/to-do against some other CRM record — a Lead,
/// Opportunity, Account, or Contact — using the same opaque (EntityType, EntityId)
/// polymorphic-reference convention as <c>Activity</c> (see Activity.cs), validated by
/// the command handler that creates it, not enforced here.
///
/// Named "FollowUpTask" rather than "Task" deliberately: every async method in this
/// codebase already uses <see cref="System.Threading.Tasks.Task"/>, and a domain type
/// literally called Task would collide with it constantly.
///
/// Unlike Activity — a point-in-time log entry that is never edited after the fact —
/// a FollowUpTask has its own tiny lifecycle: open, then Completed via
/// <see cref="Complete"/>. AssignedToUserId is an opaque cross-module reference into
/// Core's own User, never existence-validated here — same convention as
/// MaintenanceRequest.AssignedTechnicianUserId / PickList.AssignedToUserId.
/// </summary>
public sealed class FollowUpTask : TenantAggregateRoot
{
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private FollowUpTask() { }

    public static FollowUpTask Create(
        Guid companyId,
        string title,
        string? description,
        DateTimeOffset dueDate,
        string entityType,
        Guid entityId,
        Guid? assignedToUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));

        var task = new FollowUpTask
        {
            CompanyId = companyId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DueDate = dueDate,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            AssignedToUserId = assignedToUserId,
            IsCompleted = false,
        };

        task.Raise(new FollowUpTaskCreated(task.Id, companyId, task.EntityType, entityId, dueDate));
        return task;
    }

    /// <summary>Marks the task done. Requires it not already be completed — same "can't complete twice" guard idiom as MaintenanceRequest.Complete.</summary>
    public void Complete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("This follow-up task is already completed.");

        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
