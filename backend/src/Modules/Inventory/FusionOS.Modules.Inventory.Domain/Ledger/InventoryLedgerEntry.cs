using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Ledger.Events;

namespace FusionOS.Modules.Inventory.Domain.Ledger;

/// <summary>
/// Append-only stock ledger — 04_DATABASE_GUIDELINES.md §12. Valuation (FIFO,
/// Weighted Average Cost) will be computed FROM this ledger in a later slice;
/// this entry type itself is never updated or hard-deleted, only ever
/// appended to (corrections are new reversal + correction entries).
///
/// ProductId and WarehouseId are opaque references to Inventory's own Product
/// aggregate and Warehouse module's Warehouse aggregate respectively — per
/// 03_SYSTEM_ARCHITECTURE.md §2 there is no foreign key across module schemas.
/// Callers are responsible for supplying valid ids; cross-module existence
/// validation (calling Warehouse's API to confirm a WarehouseId is real) is a
/// documented follow-up, not implemented in this slice.
/// </summary>
public sealed class InventoryLedgerEntry : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityDelta { get; private set; }
    public decimal? UnitCost { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public string Reason { get; private set; } = default!;
    public DateTimeOffset TransactionDate { get; private set; }

    private InventoryLedgerEntry() { }

    /// <summary>
    /// <c>batchNumber</c>/<c>serialNumber</c> (M9 remaining — Batch/Lot/Serial tracking,
    /// 2026-07-16) are opaque, unvalidated free-text captured at the point of movement —
    /// same restraint as <see cref="Reason"/> and as GoodsReceiptLine's own BatchNumber/
    /// SerialNumber, which this factory's two producing call sites
    /// (GoodsReceiptLineReceivedConsumer, AdjustStockCommandHandler) pass straight
    /// through unchanged. A stock-out entry carrying a batch/serial is meaningful too
    /// (which lot/unit left) — this is not receipt-only.
    /// </summary>
    public static InventoryLedgerEntry RecordAdjustment(Guid companyId, Guid productId, Guid warehouseId, decimal quantityDelta, string reason, decimal? unitCost = null, string? batchNumber = null, string? serialNumber = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantityDelta == 0)
            throw new ArgumentException("Quantity delta cannot be zero — an adjustment must actually adjust something.", nameof(quantityDelta));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required for every stock adjustment (audit trail).", nameof(reason));

        var entry = new InventoryLedgerEntry
        {
            CompanyId = companyId,
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityDelta = quantityDelta,
            UnitCost = unitCost,
            BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim(),
            SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim(),
            Reason = reason.Trim(),
            TransactionDate = DateTimeOffset.UtcNow,
        };

        entry.Raise(new StockAdjusted(entry.Id, companyId, productId, warehouseId, quantityDelta));
        return entry;
    }
}
