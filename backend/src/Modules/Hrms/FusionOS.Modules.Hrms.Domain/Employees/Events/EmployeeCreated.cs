using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Employees.Events;

/// <summary>Raised on Employee creation. No consumer this slice — same deliberate restraint as Maintenance's AssetCreated.</summary>
public sealed record EmployeeCreated(Guid EmployeeId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
