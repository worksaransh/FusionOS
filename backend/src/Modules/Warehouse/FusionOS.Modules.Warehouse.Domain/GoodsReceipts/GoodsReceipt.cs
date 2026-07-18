using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts.Events;

namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

/// <summary>
/// Records goods physically received at a Warehouse/Zone, optionally against a
/// Purchase Order (03_SYSTEM_ARCHITECTURE.md §4.2's event catalog: "GoodsReceived.v1
/// | Producer: Warehouse"). PurchaseOrderId and SupplierId are opaque references
/// into Procurement's aggregates (§2) — no cross-module foreign key, same
/// documented, reviewed pattern as Inventory ledger entries referencing Product.
///
/// Not built yet: automatic Stock Ledger crediting and Purchase Order status
/// advancement both depend on a Kafka consumer subscribing to the
/// GoodsReceiptLineReceived event this aggregate raises — that consumer does not
/// exist yet (only the producer side, and the generic outbox-to-Kafka relay, are
/// wired up in this round). Until it exists, record the corresponding Stock
/// Adjustment manually via Inventory's existing endpoint.
/// </summary>
public sealed class GoodsReceipt : TenantAggregateRoot
{
    private readonly List<GoodsReceiptLine> _lines = new();

    public Guid WarehouseId { get; private set; }
    public Guid ZoneId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public DateTimeOffset ReceivedDate { get; private set; }
    public IReadOnlyList<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    private GoodsReceipt() { }

    public static GoodsReceipt Create(
        Guid companyId,
        Guid warehouseId,
        Guid zoneId,
        Guid? purchaseOrderId,
        Guid? supplierId,
        IReadOnlyCollection<GoodsReceiptLineInput> lines)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone id is required.", nameof(zoneId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A goods receipt must have at least one line.", nameof(lines));

        var receipt = new GoodsReceipt
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            ZoneId = zoneId,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = supplierId,
            ReceivedDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            receipt._lines.Add(GoodsReceiptLine.Create(line.ProductId, line.QuantityReceived, line.UnitCost, line.BatchNumber, line.SerialNumber));

        foreach (var line in receipt._lines)
        {
            receipt.Raise(new GoodsReceiptLineReceived(
                receipt.Id,
                companyId,
                line.ProductId,
                warehouseId,
                zoneId,
                line.QuantityReceived,
                line.UnitCost,
                purchaseOrderId ?? Guid.Empty,
                line.BatchNumber,
                line.SerialNumber));
        }

        return receipt;
    }

    /// <summary>
    /// Records a system-suggested putaway bin for one line (docs/IMPLEMENTATION_PLAN.md
    /// item 12). A hint only — no event, no state transition, and never a
    /// precondition for <see cref="ConfirmPutaway"/>; the caller (the command
    /// handler) is responsible for picking a real, active Bin within this
    /// receipt's own Zone before calling this.
    /// </summary>
    public void SuggestBin(Guid lineId, Guid binId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new ArgumentException($"Line '{lineId}' was not found on this goods receipt.", nameof(lineId));

        line.SuggestBin(binId);
    }

    /// <summary>
    /// Confirms the bin a line's goods were actually put away into. The caller is
    /// responsible for validating the Bin exists and belongs to this receipt's own
    /// Zone (same-module reference, unlike the opaque ProductId/PurchaseOrderId/
    /// SupplierId references) — this aggregate only enforces that the line exists.
    /// </summary>
    public void ConfirmPutaway(Guid lineId, Guid binId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new ArgumentException($"Line '{lineId}' was not found on this goods receipt.", nameof(lineId));

        line.ConfirmPutaway(binId);

        Raise(new GoodsReceiptLinePutAway(Id, CompanyId, lineId, line.ProductId, binId));
    }
}
