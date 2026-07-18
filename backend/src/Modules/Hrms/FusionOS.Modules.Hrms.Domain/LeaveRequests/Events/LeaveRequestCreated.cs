using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.LeaveRequests.Events;

public sealed record LeaveRequestCreated(Guid LeaveRequestId, Guid CompanyId, Guid EmployeeId, string Type) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
