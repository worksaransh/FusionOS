using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Maintenance's SparePartsConsumedForMaintenance domain event (relayed via the
/// outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by posting the real Stock Ledger
/// movement a recorded spare part usage implies: one negative adjustment for the consumed
/// product in the warehouse it was drawn from. This is why Maintenance never touches the
/// Inventory ledger itself — Inventory owns the ledger and applies the movement here, the
/// same producer/consumer split as WorkOrderCompletedConsumer.
///
/// Defines its own local payload shape rather than referencing Maintenance's domain event
/// CLR type — a consumer must never take a compile-time dependency on the producing
/// module's internals, only on the documented wire shape (System.Text.Json preserves the
/// original PascalCase property names since no naming policy is configured anywhere in
/// this codebase).
/// </summary>
public sealed class SparePartsConsumedForMaintenanceConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public SparePartsConsumedForMaintenanceConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "SparePartsConsumedForMaintenance";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize SparePartsConsumedForMaintenance payload for event {eventId}.");

        var reason = $"Maintenance request {payload.MaintenanceRequestId} spare part usage";

        var consumption = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            -payload.QuantityUsed,
            reason);
        await _ledgerRepository.AddAsync(consumption, cancellationToken);

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync commits the ledger entry and the idempotency marker
        // atomically — they share the same scoped DbContext instance.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid MaintenanceRequestId,
        Guid CompanyId,
        Guid ProductId,
        Guid WarehouseId,
        decimal QuantityUsed);
}
