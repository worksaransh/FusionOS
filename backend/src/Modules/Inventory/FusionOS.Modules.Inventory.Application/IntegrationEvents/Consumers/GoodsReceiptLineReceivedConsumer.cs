using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Warehouse's GoodsReceiptLineReceived domain event (relayed via the
/// outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by crediting the Inventory
/// Stock Ledger. This deliberately defines its own local payload shape rather
/// than referencing Warehouse's domain event CLR type — a consumer must never
/// take a compile-time dependency on the producing module's internals, only on
/// the documented wire shape (System.Text.Json preserves the original PascalCase
/// property names since no naming policy is configured anywhere in this codebase).
///
/// Not implemented here (documented follow-up, same as at the source event):
/// Procurement advancing the Purchase Order's received-status, and Finance's AP
/// match — both need correlation logic this consumer does not have.
/// </summary>
public sealed class GoodsReceiptLineReceivedConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public GoodsReceiptLineReceivedConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
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

        var entry = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            payload.QuantityReceived,
            $"Goods receipt {payload.GoodsReceiptId}",
            payload.UnitCost,
            payload.BatchNumber,
            payload.SerialNumber);

        await _ledgerRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry and the idempotency marker share
        // the same scoped DbContext instance behind IInventoryLedgerRepository and
        // IProcessedIntegrationEventStore, so this commits both atomically.
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
        Guid PurchaseOrderId,
        string? BatchNumber,
        string? SerialNumber);
}
