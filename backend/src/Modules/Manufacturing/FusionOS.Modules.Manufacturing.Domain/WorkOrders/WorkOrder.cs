using FusionOS.SharedKernel;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;

namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders;

/// <summary>
/// Phase 3 — Manufacturing ERP. An order to manufacture a quantity of a product from a
/// specific <see cref="BillOfMaterials"/>. Lifecycle Draft → Released → Completed
/// (with Cancel available before completion). Components are snapshotted at creation
/// from the BOM (see <see cref="WorkOrderComponent"/>), so this aggregate is
/// self-contained at completion time and needs no cross-aggregate read.
///
/// <see cref="BillOfMaterialsId"/> is a same-module reference (validated by the command
/// handler before creation); <see cref="ProductId"/> and <see cref="WarehouseId"/> are
/// opaque cross-module references into Inventory/Warehouse (never existence-validated
/// here, same convention as InventoryLedgerEntry's own WarehouseId). Completing the order
/// raises <see cref="WorkOrderCompleted"/>, which Inventory consumes to post the real
/// stock movements — this aggregate never touches the Inventory ledger itself.
/// </summary>
public sealed class WorkOrder : TenantAggregateRoot
{
    private readonly List<WorkOrderComponent> _components = new();

    public Guid BillOfMaterialsId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityToProduce { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public IReadOnlyList<WorkOrderComponent> Components => _components.AsReadOnly();

    private WorkOrder() { }

    public static WorkOrder Create(
        Guid companyId,
        Guid billOfMaterialsId,
        Guid productId,
        Guid warehouseId,
        decimal quantityToProduce,
        IReadOnlyCollection<BomComponentSnapshot> components)
    {
        if (billOfMaterialsId == Guid.Empty)
            throw new ArgumentException("Bill of materials id is required.", nameof(billOfMaterialsId));
        if (productId == Guid.Empty)
            throw new ArgumentException("Manufactured product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantityToProduce <= 0)
            throw new ArgumentException("Quantity to produce must be greater than zero.", nameof(quantityToProduce));
        if (components is null || components.Count == 0)
            throw new ArgumentException("A work order must have at least one component to consume.", nameof(components));

        var workOrder = new WorkOrder
        {
            CompanyId = companyId,
            BillOfMaterialsId = billOfMaterialsId,
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityToProduce = quantityToProduce,
            Status = WorkOrderStatus.Draft,
        };

        foreach (var component in components)
            workOrder._components.Add(WorkOrderComponent.Create(component.ComponentProductId, component.QuantityPerUnit * quantityToProduce));

        return workOrder;
    }

    /// <summary>Draft → Released: the order is committed and ready to run on the floor.</summary>
    public void Release()
    {
        if (Status != WorkOrderStatus.Draft)
            throw new InvalidOperationException($"Only a Draft work order can be released (current status: {Status}).");

        Status = WorkOrderStatus.Released;
    }

    /// <summary>
    /// Released → Completed: raises <see cref="WorkOrderCompleted"/> carrying the parent
    /// production and every component consumption, for Inventory to post to the ledger.
    /// </summary>
    public void Complete()
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Only a Released work order can be completed (current status: {Status}).");

        Status = WorkOrderStatus.Completed;
        Raise(new WorkOrderCompleted(
            Id,
            CompanyId,
            WarehouseId,
            ProductId,
            QuantityToProduce,
            _components.Select(c => new WorkOrderComponentConsumption(c.ComponentProductId, c.QuantityRequired)).ToList()));
    }

    /// <summary>Cancels a not-yet-completed work order. A completed order cannot be cancelled — its stock movements have already posted.</summary>
    public void Cancel()
    {
        if (Status == WorkOrderStatus.Completed)
            throw new InvalidOperationException("A completed work order cannot be cancelled.");
        if (Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("This work order is already cancelled.");

        Status = WorkOrderStatus.Cancelled;
    }
}

/// <summary>
/// The per-unit component figures the command handler reads off a BillOfMaterials and hands
/// to <see cref="WorkOrder.Create"/> — kept in Domain so the aggregate never takes a
/// dependency on the Application layer that assembles it.
/// </summary>
public sealed record BomComponentSnapshot(Guid ComponentProductId, decimal QuantityPerUnit);
