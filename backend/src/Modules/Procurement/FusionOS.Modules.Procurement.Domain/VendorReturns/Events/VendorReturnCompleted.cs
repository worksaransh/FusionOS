using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.VendorReturns.Events;

/// <summary>
/// Relayed via the outbox to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2) — Inventory
/// consumes this to debit the Stock Ledger (a new VendorReturnCompletedConsumer,
/// same shape as GoodsReceiptLineReceivedConsumer). Every field a downstream
/// consumer needs travels on the event itself so it never needs to look back
/// into this aggregate — same "self-carry everything" restraint as
/// PurchaseOrderGoodsReceiptCosted.
/// </summary>
public sealed record VendorReturnCompleted(Guid VendorReturnId, Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, Guid WarehouseId, decimal Quantity, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
