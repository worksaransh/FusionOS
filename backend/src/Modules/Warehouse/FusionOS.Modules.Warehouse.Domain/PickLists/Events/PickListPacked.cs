using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.PickLists.Events;

/// <summary>
/// Raised when a pick list is confirmed packed — the natural future integration point for Sales'
/// Dispatch flow ("Packing — a pack confirmation step after picking, before Dispatch is marked
/// shipped," docs/IMPLEMENTATION_PLAN.md Phase 9). Deliberately NOT wired into
/// `Dispatch.Create()` or any consumer in this phase — same restraint as ApprovalRequest not being
/// wired into `PurchaseOrder.Approve()`: retrofitting Sales' tested, working Dispatch flow to
/// require a packed PickList is a separate, later migration, not something to bundle into the
/// pick-list feature's own first slice.
/// </summary>
public sealed record PickListPacked(Guid PickListId, Guid CompanyId, Guid WarehouseId, Guid SalesOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
