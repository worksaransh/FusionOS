using System.Text.Json;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Procurement.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Warehouse's GoodsReceiptLineReceived domain event (relayed via the
/// outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by advancing the matching
/// Purchase Order's received-status — the consumer GoodsReceipt's own doc
/// comment flagged as "not built yet." Defines its own local payload shape
/// rather than referencing Warehouse's domain event CLR type, same reviewed
/// pattern as Inventory's GoodsReceiptLineReceivedConsumer: a consumer must
/// never take a compile-time dependency on the producing module's internals.
/// A goods receipt not tied to any Purchase Order (PurchaseOrderId ==
/// Guid.Empty, e.g. an ad-hoc warehouse intake) is a legitimate no-op here.
/// </summary>
public sealed class GoodsReceiptLineReceivedConsumer : IIntegrationEventConsumer
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public GoodsReceiptLineReceivedConsumer(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "GoodsReceiptLineReceived";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize GoodsReceiptLineReceived payload for event {eventId}.");

        if (payload.PurchaseOrderId != Guid.Empty)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(payload.CompanyId, payload.PurchaseOrderId, cancellationToken);
            purchaseOrder?.RecordGoodsReceipt(payload.ProductId, payload.QuantityReceived);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the purchase order update and the idempotency
        // marker share the same scoped DbContext instance behind
        // IPurchaseOrderRepository and IProcessedIntegrationEventStore, so this
        // commits both atomically (or neither, if something throws first).
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid GoodsReceiptId,
        Guid CompanyId,
        Guid ProductId,
        Guid WarehouseId,
        Guid ZoneId,
        decimal QuantityReceived,
        decimal? UnitCost,
        Guid PurchaseOrderId);
}
