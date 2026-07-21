using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Batches.Events;

namespace FusionOS.Modules.Inventory.Domain.Batches;

/// <summary>
/// Structured batch/lot tracking for a Product — sits alongside the existing
/// opaque InventoryLedgerEntry.BatchNumber/GoodsReceiptLine.BatchNumber string
/// fields (Warehouse module) without touching them: those already flow
/// through GoodsReceiptLineReceivedConsumer into the ledger today and stay
/// exactly as they are. This aggregate is the real "what do we know about
/// batch B123" record — expiry, quantity received, quantity still remaining
/// — for expiry reporting and batch-level consumption tracking that the
/// opaque ledger string alone can't answer.
///
/// ProductId is a real, same-module foreign key (Product lives in this
/// module), validated by the command handler via IProductRepository — same
/// convention as CreateMaintenanceRequest validating AssetId in Maintenance.
///
/// Consume() only tracks how much of this batch remains — it does NOT itself
/// post an InventoryLedgerEntry; the actual stock movement is still posted by
/// the existing, already-working ledger path (GoodsReceiptLineReceivedConsumer
/// and friends). Same "this is a soft/derived concern, not the movement of
/// record" restraint as Reservation not posting its own ledger entry.
/// </summary>
public sealed class Batch : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public string BatchNumber { get; private set; } = default!;
    public DateTimeOffset? ExpiryDate { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal QuantityRemaining { get; private set; }

    private Batch() { }

    public static Batch Create(Guid companyId, Guid productId, string batchNumber, decimal quantityReceived, DateTimeOffset? expiryDate = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        if (quantityReceived <= 0)
            throw new ArgumentException("Quantity received must be greater than zero.", nameof(quantityReceived));

        var batch = new Batch
        {
            CompanyId = companyId,
            ProductId = productId,
            BatchNumber = batchNumber.Trim(),
            QuantityReceived = quantityReceived,
            QuantityRemaining = quantityReceived,
            ExpiryDate = expiryDate,
        };

        batch.Raise(new BatchCreated(batch.Id, companyId, productId, batch.BatchNumber, quantityReceived));
        return batch;
    }

    /// <summary>Records that part of this batch has been used — reduces QuantityRemaining. Throws rather than going negative; the caller (not this aggregate) is responsible for the actual ledger movement, see class doc comment.</summary>
    public void Consume(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Consumption quantity must be greater than zero.", nameof(quantity));
        if (quantity > QuantityRemaining)
            throw new InvalidOperationException($"Cannot consume {quantity} from batch '{BatchNumber}' — only {QuantityRemaining} remains.");

        QuantityRemaining -= quantity;
    }

    /// <summary>Corrects the recorded expiry (e.g. a data-entry fix, or a supplier-confirmed shelf-life extension). Null clears it — not every product tracks expiry.</summary>
    public void AdjustExpiry(DateTimeOffset? newExpiry) => ExpiryDate = newExpiry;
}
