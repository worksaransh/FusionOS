using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Payroll.Events;

/// <summary>Raised once a payroll draft is created. No consumer this slice — same deliberate restraint as EmployeeCreated/LeaveRequestCreated/AttendanceRecorded.</summary>
public sealed record PayrollRecordDrafted(Guid PayrollRecordId, Guid CompanyId, Guid EmployeeId, int PeriodMonth, int PeriodYear) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
