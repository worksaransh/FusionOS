using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Workflow.Events;

public sealed record ApprovalRequestSubmitted(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, Guid RequestedBy, Guid FirstApproverUserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
