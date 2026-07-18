using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Manufacturing's WorkOrderCompleted domain event (relayed via the outbox
/// to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by posting the real Stock Ledger
/// movements a completed work order implies: one negative adjustment per consumed
/// component and one positive adjustment for the produced parent product, all in the
/// work order's warehouse. This is why Manufacturing never touches the Inventory ledger
/// itself — Inventory owns the ledger and applies the movement here, the same
/// producer/consumer split as GoodsReceiptLineReceivedConsumer.
///
/// Defines its own local payload shape rather than referencing Manufacturing's domain
/// event CLR type — a consumer must never take a compile-time dependency on the
/// producing module's internals, only on the documented wire shape (System.Text.Json
/// preserves the original PascalCase property names since no naming policy is
/// configured anywhere in this codebase).
/// </summary>
public sealed class WorkOrderCompletedConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public WorkOrderCompletedConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "WorkOrderCompleted";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize WorkOrderCompleted payload for event {eventId}.");

        var reason = $"Work order {payload.WorkOrderId} completion";

        // Consume each component: a negative delta out of stock.
        foreach (var component in payload.Components ?? Array.Empty<ComponentPayload>())
        {
            var consumption = InventoryLedgerEntry.RecordAdjustment(
                payload.CompanyId,
                component.ComponentProductId,
                payload.WarehouseId,
                -component.QuantityConsumed,
                reason);
            await _ledgerRepository.AddAsync(consumption, cancellationToken);
        }

        // Produce the parent product: a positive delta into stock.
        var production = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            payload.QuantityProduced,
            reason);
        await _ledgerRepository.AddAsync(production, cancellationToken);

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync commits every ledger entry and the idempotency marker
        // atomically — they share the same scoped DbContext instance.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid WorkOrderId,
        Guid CompanyId,
        Guid WarehouseId,
        Guid ProductId,
        decimal QuantityProduced,
        IReadOnlyList<ComponentPayload> Components);

    private sealed record ComponentPayload(Guid ComponentProductId, decimal QuantityConsumed);
}
