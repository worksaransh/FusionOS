using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Onboarding.Events;

/// <summary>Raised once an onboarding or offboarding workflow is started. No consumer this slice — same deliberate restraint as EmployeeCreated/LeaveRequestCreated/AttendanceRecorded/PayrollRecordDrafted.</summary>
public sealed record EmployeeWorkflowStarted(Guid EmployeeWorkflowId, Guid CompanyId, Guid EmployeeId, string Kind) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
