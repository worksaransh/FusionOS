using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Workflow.Events;

public sealed record ApprovalRequestRejected(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, Guid RequestedBy, int RejectedAtStepNumber) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
