using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Attendance.Events;

/// <summary>Raised once an attendance record is created. No consumer this slice — same deliberate restraint as EmployeeCreated/LeaveRequestCreated.</summary>
public sealed record AttendanceRecorded(Guid AttendanceRecordId, Guid CompanyId, Guid EmployeeId, string Status) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
