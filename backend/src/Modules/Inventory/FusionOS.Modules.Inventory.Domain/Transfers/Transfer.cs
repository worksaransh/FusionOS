using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Transfers.Events;

namespace FusionOS.Modules.Inventory.Domain.Transfers;

/// <summary>
/// Phase 1 closeout (2026-07-18): moves a quantity of a Product from one
/// Warehouse to another — 05_MODULE_ROADMAP.md's Inventory "Transfers" line
/// item, confirmed absent by a repo-wide grep before this slice. Pending →
/// Completed (the actual stock movement is posted as two InventoryLedgerEntry
/// rows — a negative delta at the source, a positive delta at the
/// destination — by the completing command handler, same "aggregate raises
/// the event, the Application-layer handler does the cross-aggregate write"
/// split as Reservation.Fulfill()) or Cancelled (no stock movement at all).
///
/// SourceWarehouseId/DestinationWarehouseId are opaque cross-module
/// references into the Warehouse module's own Warehouse aggregate, same
/// convention as InventoryLedgerEntry's own WarehouseId — never
/// existence-validated here.
/// </summary>
public sealed class Transfer : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public TransferStatus Status { get; private set; }
    public DateTimeOffset TransferDate { get; private set; }

    private Transfer() { }

    public static Transfer Create(Guid companyId, Guid productId, Guid sourceWarehouseId, Guid destinationWarehouseId, decimal quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (sourceWarehouseId == Guid.Empty)
            throw new ArgumentException("Source warehouse id is required.", nameof(sourceWarehouseId));
        if (destinationWarehouseId == Guid.Empty)
            throw new ArgumentException("Destination warehouse id is required.", nameof(destinationWarehouseId));
        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different.", nameof(destinationWarehouseId));
        if (quantity <= 0)
            throw new ArgumentException("Transfer quantity must be greater than zero.", nameof(quantity));

        var transfer = new Transfer
        {
            CompanyId = companyId,
            ProductId = productId,
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Quantity = quantity,
            Status = TransferStatus.Pending,
            TransferDate = DateTimeOffset.UtcNow,
        };

        transfer.Raise(new TransferCreated(transfer.Id, companyId, productId, sourceWarehouseId, destinationWarehouseId, quantity));
        return transfer;
    }

    /// <summary>Marks the transfer as moved — the caller (CompleteTransferCommandHandler) is responsible for checking source stock and posting the two ledger entries.</summary>
    public void Complete()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Only a Pending transfer can be completed (current status: {Status}).");

        Status = TransferStatus.Completed;
        Raise(new TransferCompleted(Id, CompanyId, ProductId, SourceWarehouseId, DestinationWarehouseId, Quantity));
    }

    /// <summary>Cancels the transfer without any stock movement.</summary>
    public void Cancel()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Only a Pending transfer can be cancelled (current status: {Status}).");

        Status = TransferStatus.Cancelled;
    }
}
