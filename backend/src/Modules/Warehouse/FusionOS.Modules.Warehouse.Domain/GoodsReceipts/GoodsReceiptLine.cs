namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

/// <summary>
/// A line item within a GoodsReceipt aggregate. Documented, reviewed exception to
/// the "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as PurchaseOrderLine/SalesOrderLine: a line's lifecycle is owned
/// entirely by its parent GoodsReceipt. ProductId is an opaque reference into
/// Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module
/// foreign key.
///
/// <c>SuggestedBinId</c>/<c>PutAwayBinId</c> and their two mutators
/// (<see cref="SuggestBin"/>/<see cref="ConfirmPutaway"/>) are a deliberate,
/// documented deviation from this line's original fully-immutable shape (no
/// mutators at all, matching PurchaseOrderLine/SalesOrderLine) — same reasoning
/// as PickListLine.RecordPicked in the Picking+Packing slice: putaway is filled in
/// progressively after the line already exists (goods are received first, a bin
/// is suggested/confirmed afterward), not a new document per change. BinId is a
/// same-module reference to Bin (Warehouse), so unlike ProductId it **is**
/// validated for existence by the command handlers that call these mutators.
/// </summary>
public sealed class GoodsReceiptLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal? UnitCost { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public Guid? SuggestedBinId { get; private set; }
    public Guid? PutAwayBinId { get; private set; }
    public bool IsPutAway => PutAwayBinId.HasValue;

    private GoodsReceiptLine() { }

    internal static GoodsReceiptLine Create(Guid productId, decimal quantityReceived, decimal? unitCost, string? batchNumber = null, string? serialNumber = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantityReceived <= 0)
            throw new ArgumentException("Quantity received must be greater than zero.", nameof(quantityReceived));
        if (unitCost is < 0)
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));
        if (batchNumber is { Length: > 100 })
            throw new ArgumentException("Batch number cannot exceed 100 characters.", nameof(batchNumber));
        if (serialNumber is { Length: > 100 })
            throw new ArgumentException("Serial number cannot exceed 100 characters.", nameof(serialNumber));

        return new GoodsReceiptLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            QuantityReceived = quantityReceived,
            UnitCost = unitCost,
            BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim(),
            SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim(),
        };
    }

    /// <summary>Records a system-suggested bin — a hint only, never required before <see cref="ConfirmPutaway"/>.</summary>
    internal void SuggestBin(Guid binId)
    {
        if (binId == Guid.Empty)
            throw new ArgumentException("Bin id is required.", nameof(binId));

        SuggestedBinId = binId;
    }

    /// <summary>
    /// Records the bin goods were actually put away into — independent of any prior
    /// suggestion (a worker can override it). Re-confirming overwrites the previous
    /// value, same "record what's true now" semantics as PickListLine.RecordPicked
    /// and CycleCount.RecordCount — a corrected re-entry never leaves stale state.
    /// </summary>
    internal void ConfirmPutaway(Guid binId)
    {
        if (binId == Guid.Empty)
            throw new ArgumentException("Bin id is required.", nameof(binId));

        PutAwayBinId = binId;
    }
}
