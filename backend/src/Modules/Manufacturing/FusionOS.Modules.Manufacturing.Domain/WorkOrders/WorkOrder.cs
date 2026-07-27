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
    private readonly List<WorkOrderOperation> _operations = new();

    public Guid BillOfMaterialsId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityToProduce { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public IReadOnlyList<WorkOrderComponent> Components => _components.AsReadOnly();

    /// <summary>This order's routing operations, snapshotted from the BOM at creation (Phase 6, 2026-07-22) — always in SequenceNumber order, same convention as BillOfMaterials.Operations.</summary>
    public IReadOnlyList<WorkOrderOperation> Operations => _operations.OrderBy(o => o.SequenceNumber).ToList();

    /// <summary>The completed work order this one reworks the scrapped quantity of — null unless this order was created by CreateReworkWorkOrderCommand (Rework, Phase 6).</summary>
    public Guid? SourceWorkOrderId { get; private set; }

    /// <summary>How much of this (Completed) order's QuantityScrapped has already been sent into a rework order — never exceeds QuantityScrapped.</summary>
    public decimal QuantityReworked { get; private set; }

    /// <summary>The actual good (non-scrap) quantity produced — set only once this order is Completed; null until then.</summary>
    public decimal? QuantityGoodProduced { get; private set; }

    /// <summary>The quantity scrapped at completion — zero unless recorded otherwise.</summary>
    public decimal QuantityScrapped { get; private set; }

    /// <summary>Good ÷ (Good + Scrapped) × 100, rounded to 2 decimal places — set only once this order is Completed; null until then.</summary>
    public decimal? YieldPercentage { get; private set; }

    private WorkOrder() { }

    public static WorkOrder Create(
        Guid companyId,
        Guid billOfMaterialsId,
        Guid productId,
        Guid warehouseId,
        decimal quantityToProduce,
        IReadOnlyCollection<BomComponentSnapshot> components,
        IReadOnlyCollection<RoutingOperationSnapshot>? operations = null,
        Guid? sourceWorkOrderId = null)
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
            SourceWorkOrderId = sourceWorkOrderId,
        };

        foreach (var component in components)
        {
            workOrder._components.Add(WorkOrderComponent.Create(
                component.ComponentProductId, component.QuantityPerUnit * quantityToProduce, component.UnitCost));
        }

        foreach (var operation in operations ?? Array.Empty<RoutingOperationSnapshot>())
        {
            workOrder._operations.Add(WorkOrderOperation.Create(
                operation.RoutingOperationId, operation.SequenceNumber, operation.OperationName, operation.WorkCenter, operation.StandardMinutes));
        }

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
    ///
    /// <paramref name="quantityGoodProduced"/> and <paramref name="quantityScrapped"/> are
    /// optional scrap/yield recording: when omitted, this behaves exactly as a plain
    /// Complete() always has (100% yield — the full planned QuantityToProduce, zero
    /// scrap) so every existing caller is unaffected. QuantityProduced on the raised
    /// event is always the GOOD quantity, since that is the only amount Inventory
    /// should post into stock — scrapped units never post to the ledger.
    /// </summary>
    public void Complete(decimal? quantityGoodProduced = null, decimal? quantityScrapped = null)
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Only a Released work order can be completed (current status: {Status}).");

        var good = quantityGoodProduced ?? QuantityToProduce;
        var scrapped = quantityScrapped ?? 0m;

        if (good < 0)
            throw new ArgumentException("Good quantity produced cannot be negative.", nameof(quantityGoodProduced));
        if (scrapped < 0)
            throw new ArgumentException("Scrapped quantity cannot be negative.", nameof(quantityScrapped));
        if (good == 0 && scrapped == 0)
            throw new ArgumentException("A completed work order must account for at least some produced or scrapped quantity.", nameof(quantityGoodProduced));

        Status = WorkOrderStatus.Completed;
        QuantityGoodProduced = good;
        QuantityScrapped = scrapped;
        YieldPercentage = Math.Round(good / (good + scrapped) * 100m, 2);

        Raise(new WorkOrderCompleted(
            Id,
            CompanyId,
            WarehouseId,
            ProductId,
            good,
            _components.Select(c => new WorkOrderComponentConsumption(c.ComponentProductId, c.QuantityRequired)).ToList(),
            scrapped,
            YieldPercentage.Value));
    }

    /// <summary>
    /// Issues (consumes on the shop floor) a quantity of one snapshotted component ahead
    /// of this order's eventual completion — a floor-level progress signal distinct from
    /// the final stock consumption WorkOrderCompleted implies. Only permitted while
    /// Released; cannot push a component's issued total past its snapshotted QuantityRequired.
    /// </summary>
    public void IssueMaterial(Guid componentProductId, decimal quantity)
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Material can only be issued to a Released work order (current status: {Status}).");
        if (quantity <= 0)
            throw new ArgumentException("Issue quantity must be greater than zero.", nameof(quantity));

        var component = _components.FirstOrDefault(c => c.ComponentProductId == componentProductId)
            ?? throw new ArgumentException($"Component '{componentProductId}' is not part of this work order.", nameof(componentProductId));

        component.Issue(quantity);
        Raise(new MaterialIssuedToWorkOrder(Id, CompanyId, WarehouseId, componentProductId, quantity));
    }

    /// <summary>Returns previously issued material for a component — the inverse of <see cref="IssueMaterial"/>. Only permitted while Released.</summary>
    public void ReturnMaterial(Guid componentProductId, decimal quantity)
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Material can only be returned on a Released work order (current status: {Status}).");
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));

        var component = _components.FirstOrDefault(c => c.ComponentProductId == componentProductId)
            ?? throw new ArgumentException($"Component '{componentProductId}' is not part of this work order.", nameof(componentProductId));

        component.Return(quantity);
        Raise(new MaterialReturnedFromWorkOrder(Id, CompanyId, WarehouseId, componentProductId, quantity));
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

    /// <summary>Starts one of this order's snapshotted routing operations (Phase 6, 2026-07-22). Only permitted while the order itself is Released.</summary>
    public void StartOperation(Guid workOrderOperationId, DateTimeOffset now)
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Operations can only be started on a Released work order (current status: {Status}).");

        var operation = _operations.FirstOrDefault(o => o.Id == workOrderOperationId)
            ?? throw new ArgumentException($"Operation '{workOrderOperationId}' is not part of this work order.", nameof(workOrderOperationId));

        operation.Start(now);
    }

    /// <summary>Completes one of this order's routing operations. Only permitted while the order itself is Released — does not itself complete the whole work order.</summary>
    public void CompleteOperation(Guid workOrderOperationId, DateTimeOffset now)
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException($"Operations can only be completed on a Released work order (current status: {Status}).");

        var operation = _operations.FirstOrDefault(o => o.Id == workOrderOperationId)
            ?? throw new ArgumentException($"Operation '{workOrderOperationId}' is not part of this work order.", nameof(workOrderOperationId));

        operation.Complete(now);
    }

    /// <summary>
    /// Assigns a machine and time window to one of this order's routing operations
    /// (Machine Scheduling, Phase 6). Cross-work-order conflict checking (does this
    /// machine already have an overlapping schedule elsewhere) is the command
    /// handler's job — it must query across every work order, which this aggregate
    /// cannot see.
    /// </summary>
    public void ScheduleOperation(Guid workOrderOperationId, Guid machineId, DateTimeOffset scheduledStart, DateTimeOffset scheduledEnd)
    {
        var operation = _operations.FirstOrDefault(o => o.Id == workOrderOperationId)
            ?? throw new ArgumentException($"Operation '{workOrderOperationId}' is not part of this work order.", nameof(workOrderOperationId));

        operation.Schedule(machineId, scheduledStart, scheduledEnd);
    }

    /// <summary>
    /// Records that <paramref name="quantity"/> of this (Completed) order's scrapped
    /// output has been sent into a rework order (Rework, Phase 6) — the actual new
    /// production run is a separate WorkOrder (see CreateReworkWorkOrderCommandHandler),
    /// linked back via its SourceWorkOrderId; this just tracks how much of THIS order's
    /// scrap has already been claimed, so the same scrapped units can't be reworked twice.
    /// </summary>
    public void RecordRework(decimal quantity)
    {
        if (Status != WorkOrderStatus.Completed)
            throw new InvalidOperationException($"Only a Completed work order's scrap can be reworked (current status: {Status}).");
        if (quantity <= 0)
            throw new ArgumentException("Rework quantity must be greater than zero.", nameof(quantity));

        var newTotal = QuantityReworked + quantity;
        if (newTotal > QuantityScrapped)
        {
            throw new InvalidOperationException(
                $"Cannot rework {quantity}: only {QuantityScrapped - QuantityReworked} of this order's scrapped quantity remains unclaimed.");
        }

        QuantityReworked = newTotal;
    }
}

/// <summary>
/// The per-unit component figures the command handler reads off a BillOfMaterials and hands
/// to <see cref="WorkOrder.Create"/> — kept in Domain so the aggregate never takes a
/// dependency on the Application layer that assembles it. UnitCost (WIP support, Phase 6)
/// is copied straight from BomLine.UnitCost.
/// </summary>
public sealed record BomComponentSnapshot(Guid ComponentProductId, decimal QuantityPerUnit, decimal UnitCost = 0m);

/// <summary>The routing operation figures the command handler reads off a BillOfMaterials and hands to <see cref="WorkOrder.Create"/> (Phase 6, 2026-07-22) — same "assembled by Application, defined in Domain" split as BomComponentSnapshot.</summary>
public sealed record RoutingOperationSnapshot(Guid RoutingOperationId, int SequenceNumber, string OperationName, string WorkCenter, decimal StandardMinutes);
