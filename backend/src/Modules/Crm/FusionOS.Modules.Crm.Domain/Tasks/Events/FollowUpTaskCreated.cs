using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Tasks.Events;

/// <summary>Raised when a follow-up task is created. No consumer today — a natural future hook for reminder/notification fan-out.</summary>
public sealed record FollowUpTaskCreated(Guid FollowUpTaskId, Guid CompanyId, string EntityType, Guid EntityId, DateTimeOffset DueDate) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
