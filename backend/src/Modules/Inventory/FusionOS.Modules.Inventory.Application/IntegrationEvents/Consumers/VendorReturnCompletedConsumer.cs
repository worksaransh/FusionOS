using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Procurement's VendorReturnCompleted domain event (relayed via the
/// outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by debiting the Inventory
/// Stock Ledger. Same "own local payload shape, never the producing module's
/// CLR event type" restraint as GoodsReceiptLineReceivedConsumer — Inventory
/// has no project reference to Procurement, only to the documented wire shape.
/// Procurement's VendorReturn.Complete() itself never touches this module's
/// ledger directly (it can't — module isolation is compile-time enforced),
/// which is why this consumer exists at all instead of an in-process call.
/// </summary>
public sealed class VendorReturnCompletedConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public VendorReturnCompletedConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "VendorReturnCompleted";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize VendorReturnCompleted payload for event {eventId}.");

        var entry = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            -payload.Quantity,
            $"Vendor return {payload.VendorReturnId}: {payload.Reason}");

        await _ledgerRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry and the idempotency marker share
        // the same scoped DbContext instance behind IInventoryLedgerRepository and
        // IProcessedIntegrationEventStore, so this commits both atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid VendorReturnId,
        Guid CompanyId,
        Guid PurchaseOrderId,
        Guid ProductId,
        Guid WarehouseId,
        decimal Quantity,
        string Reason);
}
