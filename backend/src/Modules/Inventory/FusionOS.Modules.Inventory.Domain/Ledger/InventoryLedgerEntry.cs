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
    public string Reason { get; private set; } = default!;
    public DateTimeOffset TransactionDate { get; private set; }

    private InventoryLedgerEntry() { }

    public static InventoryLedgerEntry RecordAdjustment(Guid companyId, Guid productId, Guid warehouseId, decimal quantityDelta, string reason, decimal? unitCost = null)
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
            Reason = reason.Trim(),
            TransactionDate = DateTimeOffset.UtcNow,
        };

        entry.Raise(new StockAdjusted(entry.Id, companyId, productId, warehouseId, quantityDelta));
        return entry;
    }
}
