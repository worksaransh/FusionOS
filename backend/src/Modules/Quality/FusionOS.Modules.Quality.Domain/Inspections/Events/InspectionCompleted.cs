using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.Inspections.Events;

/// <summary>Raised when an inspection is opened (Pending). Kept for consistency; no consumer today.</summary>
public sealed record InspectionCreated(Guid InspectionId, Guid CompanyId, string InspectionType, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Raised when an inspection's results are recorded and it resolves to Passed or Failed.
/// No consumer today — the natural future hook is a failed IncomingGoods inspection
/// quarantining the received stock (Inventory/Warehouse) or a failed Production inspection
/// flagging the work order, both deliberately out of scope for this first slice.
/// </summary>
public sealed record InspectionCompleted(Guid InspectionId, Guid CompanyId, string InspectionType, Guid ReferenceId, bool Passed) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
