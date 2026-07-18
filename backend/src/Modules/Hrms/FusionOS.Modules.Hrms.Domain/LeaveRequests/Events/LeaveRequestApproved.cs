using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.LeaveRequests.Events;

/// <summary>Raised once a leave request is approved. No consumer this slice — the natural future hook for Attendance once that module exists.</summary>
public sealed record LeaveRequestApproved(Guid LeaveRequestId, Guid CompanyId, Guid EmployeeId, DateTimeOffset StartDate, DateTimeOffset EndDate) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
