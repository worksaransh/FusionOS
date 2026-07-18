using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.CycleCounts.Events;

namespace FusionOS.Modules.Warehouse.Domain.CycleCounts;

/// <summary>
/// Warehouse-side cycle counting (docs/IMPLEMENTATION_PLAN.md Phase 9: "Cycle
/// counting (warehouse side) — same concept as Inventory's, scoped to a
/// warehouse/zone/bin"). A two-step lifecycle: Start snapshots the system
/// quantity the caller read from Inventory's GET /stock/on-hand at the moment
/// counting begins (this engine has no cross-module read of its own — no
/// direct FK/query into Inventory's schema, 03_SYSTEM_ARCHITECTURE.md §2 —
/// so the snapshot is supplied, exactly like ApprovalRequest's approver list
/// is supplied by the caller rather than decided by the engine); RecordCount
/// then submits what was physically counted, computes the variance, and —
/// only if there IS a variance — raises CycleCountVarianceRecorded for
/// Inventory to react to. A balanced count (variance == 0) completes with no
/// event; there is nothing for the ledger to adjust.
///
/// ProductId/WarehouseId are opaque cross-module references (same convention
/// as InventoryLedgerEntry); ZoneId/BinId are real in-module FKs since Zone
/// and Bin both live in this same Warehouse module.
/// </summary>
public sealed class CycleCount : TenantAggregateRoot
{
    public Guid WarehouseId { get; private set; }
    public Guid ZoneId { get; private set; }
    public Guid BinId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid StartedBy { get; private set; }
    public decimal SystemQuantitySnapshot { get; private set; }
    public decimal? CountedQuantity { get; private set; }
    public decimal? VarianceQuantity { get; private set; }
    public CycleCountStatus Status { get; private set; }

    private CycleCount() { }

    public static CycleCount Start(Guid companyId, Guid warehouseId, Guid zoneId, Guid binId, Guid productId, decimal systemQuantitySnapshot, Guid startedBy)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone id is required.", nameof(zoneId));
        if (binId == Guid.Empty)
            throw new ArgumentException("Bin id is required.", nameof(binId));
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (systemQuantitySnapshot < 0)
            throw new ArgumentException("System quantity snapshot cannot be negative.", nameof(systemQuantitySnapshot));
        if (startedBy == Guid.Empty)
            throw new ArgumentException("Started-by user id is required.", nameof(startedBy));

        var cycleCount = new CycleCount
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            BinId = binId,
            ProductId = productId,
            SystemQuantitySnapshot = systemQuantitySnapshot,
            StartedBy = startedBy,
            Status = CycleCountStatus.Pending,
        };

        cycleCount.Raise(new CycleCountStarted(cycleCount.Id, companyId, warehouseId, zoneId, binId, productId));
        return cycleCount;
    }

    /// <summary>
    /// Submits the physically-counted quantity. Can only happen once per
    /// cycle count (Status guards re-submission, same idea as
    /// ApprovalRequest.Decide rejecting a second decision on an already-
    /// resolved request).
    /// </summary>
    public void RecordCount(decimal countedQuantity)
    {
        if (Status != CycleCountStatus.Pending)
            throw new InvalidOperationException("This cycle count has already been completed.");
        if (countedQuantity < 0)
            throw new ArgumentException("Counted quantity cannot be negative.", nameof(countedQuantity));

        CountedQuantity = countedQuantity;
        VarianceQuantity = countedQuantity - SystemQuantitySnapshot;
        Status = CycleCountStatus.Completed;

        if (VarianceQuantity.Value != 0)
        {
            Raise(new CycleCountVarianceRecorded(Id, CompanyId, ProductId, WarehouseId, VarianceQuantity.Value));
        }
    }
}
