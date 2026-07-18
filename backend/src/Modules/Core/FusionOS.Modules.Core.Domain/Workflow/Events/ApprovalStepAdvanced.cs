using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Workflow.Events;

/// <summary>Raised when one step is approved but the chain isn't fully approved yet — carries the next approver so a caller can notify them.</summary>
public sealed record ApprovalStepAdvanced(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, int NextStepNumber, Guid NextApproverUserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
