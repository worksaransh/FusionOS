using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Workflow.Events;

public sealed record ApprovalRequestApproved(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, Guid RequestedBy) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
