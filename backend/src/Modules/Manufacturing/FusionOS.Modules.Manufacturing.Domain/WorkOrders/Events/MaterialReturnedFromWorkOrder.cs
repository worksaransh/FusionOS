using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;

/// <summary>
/// Raised when previously issued material is returned (unused) against a Released work
/// order's component — the inverse of <see cref="MaterialIssuedToWorkOrder"/>. Same
/// "no consumer today" pattern; Manufacturing-side progress signal only.
/// </summary>
public sealed record MaterialReturnedFromWorkOrder(
    Guid WorkOrderId,
    Guid CompanyId,
    Guid WarehouseId,
    Guid ComponentProductId,
    decimal QuantityReturned) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
