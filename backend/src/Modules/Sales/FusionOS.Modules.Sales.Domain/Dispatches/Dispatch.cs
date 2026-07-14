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
/// </summary>
public sealed class Dispatch : TenantAggregateRoot
{
    private readonly List<DispatchLine> _lines = new();

    public Guid SalesOrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTimeOffset DispatchDate { get; private set; }
    public IReadOnlyList<DispatchLine> Lines => _lines.AsReadOnly();

    private Dispatch() { }

    public static Dispatch Create(Guid companyId, Guid salesOrderId, Guid warehouseId, IReadOnlyCollection<DispatchLineInput> lines)
    {
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A dispatch must have at least one line.", nameof(lines));

        var dispatch = new Dispatch
        {
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            WarehouseId = warehouseId,
            DispatchDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            dispatch._lines.Add(DispatchLine.Create(line.ProductId, line.QuantityDispatched));

        foreach (var line in dispatch._lines)
            dispatch.Raise(new DispatchLineDispatched(dispatch.Id, companyId, salesOrderId, line.ProductId, warehouseId, line.QuantityDispatched));

        return dispatch;
    }
}
