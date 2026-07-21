using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;

/// <summary>
/// Raised when material is issued (picked and consumed on the shop floor) against a
/// Released work order's component, ahead of that order's eventual completion. Purely
/// a Manufacturing-side progress signal today — no consumer exists yet, same "no
/// consumer today" pattern as BillOfMaterialsCreated. This does not itself move
/// Inventory stock; the real ledger movement still only happens at WorkOrderCompleted.
/// </summary>
public sealed record MaterialIssuedToWorkOrder(
    Guid WorkOrderId,
    Guid CompanyId,
    Guid WarehouseId,
    Guid ComponentProductId,
    decimal QuantityIssued) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
