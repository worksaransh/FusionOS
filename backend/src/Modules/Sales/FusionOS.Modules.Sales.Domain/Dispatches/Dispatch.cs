using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Dispatches.Events;

namespace FusionOS.Modules.Sales.Domain.Dispatches;

/// <summary>
/// Records goods dispatched against a Sales Order (05_MODULE_ROADMAP.md Phase 1:
/// Sales capability list — "Dispatch"). SalesOrderId is a real, same-module
/// foreign key; WarehouseId is an opaque cross-module reference
/// (03_SYSTEM_ARCHITECTURE.md §2) — no FK, same documented pattern as
/// GoodsReceipt referencing a Purchase Order. The physical pick/pack/ship
/// execution (Warehouse's own "Dispatch" capability) is a separate, not-yet-built
/// Warehouse-side slice this aggregate's event is intended to eventually drive.
///
/// PickListId (2026-07-21 Dispatch-PickList link follow-up) is an OPAQUE, optional
/// cross-module reference into Warehouse's own PickList aggregate — same no-cross-module-FK
/// convention as WarehouseId above and as PurchaseOrderFact's PurchaseOrderId/SupplierId
/// (Finance): Sales never queries Warehouse's tables or types, so this module does not (and
/// cannot, without a compile-time reference into Warehouse.Application/Domain that no other
/// cross-module relationship in this codebase takes) validate that the PickListId exists or
/// that it belongs to the same WarehouseId/SalesOrderId as this Dispatch. It is nullable and
/// defaults to null because a Dispatch can still be recorded without ever having gone through
/// Warehouse's Picking+Packing flow (the two flows are independent today) — callers that do
/// have a real PickList id (the frontend, which already fetches pick lists via Warehouse's own
/// API) are responsible for supplying one; validating it end-to-end is a documented follow-up,
/// not implemented in this slice.
/// </summary>
public sealed class Dispatch : TenantAggregateRoot
{
    private readonly List<DispatchLine> _lines = new();

    public Guid SalesOrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? PickListId { get; private set; }
    public DateTimeOffset DispatchDate { get; private set; }
    public IReadOnlyList<DispatchLine> Lines => _lines.AsReadOnly();

    private Dispatch() { }

    public static Dispatch Create(Guid companyId, Guid salesOrderId, Guid warehouseId, IReadOnlyCollection<DispatchLineInput> lines, Guid? pickListId = null)
    {
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A dispatch must have at least one line.", nameof(lines));
        if (pickListId == Guid.Empty)
            throw new ArgumentException("Pick list id, when supplied, cannot be an empty guid.", nameof(pickListId));

        var dispatch = new Dispatch
        {
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            WarehouseId = warehouseId,
            PickListId = pickListId,
            DispatchDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            dispatch._lines.Add(DispatchLine.Create(line.ProductId, line.QuantityDispatched));

        foreach (var line in dispatch._lines)
            dispatch.Raise(new DispatchLineDispatched(dispatch.Id, companyId, salesOrderId, line.ProductId, warehouseId, line.QuantityDispatched));

        return dispatch;
    }
}
