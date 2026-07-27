using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Onboarding.Events;

/// <summary>Raised when every task on a workflow is done and it auto-transitions to Completed (see EmployeeWorkflow.CompleteTask's own doc comment). No consumer this slice.</summary>
public sealed record EmployeeWorkflowCompleted(Guid EmployeeWorkflowId, Guid CompanyId, Guid EmployeeId, string Kind) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
