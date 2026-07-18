using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.VendorReturns.Events;

namespace FusionOS.Modules.Procurement.Domain.VendorReturns;

/// <summary>
/// Phase 1 closeout (2026-07-18): sends a quantity of a Product back to the
/// supplier against a PurchaseOrder — 05_MODULE_ROADMAP.md's Procurement
/// "Vendor returns" line item, confirmed absent by a repo-wide grep before
/// this slice. Pending → Completed (posts a real stock debit) or Cancelled
/// (no stock movement).
///
/// PurchaseOrderId/ProductId are same-module/opaque references respectively
/// (PurchaseOrderId is existence-validated by the creating command handler
/// against Procurement's own PurchaseOrder aggregate; ProductId is Inventory's,
/// never validated here — same convention as PurchaseOrderLine's own
/// ProductId). WarehouseId is an opaque cross-module reference into the
/// Warehouse module — this aggregate has no way to derive it from the
/// PurchaseOrder (goods receipt, not the PO itself, is where a warehouse gets
/// recorded), so the caller supplies it directly, same pattern as
/// Reservation/Transfer's own WarehouseId.
///
/// Unlike Inventory's Transfer, this aggregate CANNOT post its own
/// InventoryLedgerEntry directly — Procurement has no project reference to
/// Inventory (03_SYSTEM_ARCHITECTURE.md §2, module isolation is enforced at
/// compile time). Complete() only flips status and raises VendorReturnCompleted;
/// the actual stock debit happens asynchronously in a new Inventory-side
/// consumer reacting to that event, same "outbox → Kafka → consumer" pattern
/// as GoodsReceiptLineReceivedConsumer.
/// </summary>
public sealed class VendorReturn : TenantAggregateRoot
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reason { get; private set; } = default!;
    public VendorReturnStatus Status { get; private set; }
    public DateTimeOffset ReturnDate { get; private set; }

    private VendorReturn() { }

    public static VendorReturn Create(Guid companyId, Guid purchaseOrderId, Guid productId, Guid warehouseId, decimal quantity, string reason)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantity <= 0)
            throw new ArgumentException("Return quantity must be greater than zero.", nameof(quantity));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required for every vendor return (audit trail).", nameof(reason));

        var vendorReturn = new VendorReturn
        {
            CompanyId = companyId,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            Reason = reason.Trim(),
            Status = VendorReturnStatus.Pending,
            ReturnDate = DateTimeOffset.UtcNow,
        };

        vendorReturn.Raise(new VendorReturnCreated(vendorReturn.Id, companyId, purchaseOrderId, productId, quantity));
        return vendorReturn;
    }

    /// <summary>Marks the return as sent back — the caller (an Inventory-side consumer reacting to VendorReturnCompleted) is responsible for the actual ledger debit.</summary>
    public void Complete()
    {
        if (Status != VendorReturnStatus.Pending)
            throw new InvalidOperationException($"Only a Pending vendor return can be completed (current status: {Status}).");

        Status = VendorReturnStatus.Completed;
        Raise(new VendorReturnCompleted(Id, CompanyId, PurchaseOrderId, ProductId, WarehouseId, Quantity, Reason));
    }

    /// <summary>Cancels the return without any stock movement.</summary>
    public void Cancel()
    {
        if (Status != VendorReturnStatus.Pending)
            throw new InvalidOperationException($"Only a Pending vendor return can be cancelled (current status: {Status}).");

        Status = VendorReturnStatus.Cancelled;
    }
}
