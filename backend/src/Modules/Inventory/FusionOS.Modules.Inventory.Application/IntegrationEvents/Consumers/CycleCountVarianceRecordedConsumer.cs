using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Warehouse's CycleCountVarianceRecorded domain event (same
/// outbox -> Kafka relay as GoodsReceiptLineReceivedConsumer) by posting a
/// stock ledger adjustment for whatever variance a physical count found.
/// Defines its own local payload shape rather than referencing Warehouse's
/// domain event CLR type, same decoupling rule as every other consumer here.
///
/// Reuses InventoryLedgerEntry.RecordAdjustment — the same factory the manual
/// "Adjust Stock" feature and the GoodsReceipt consumer both call — so a
/// cycle-count-driven adjustment shows up in the ledger indistinguishably
/// from any other adjustment except for its Reason text.
/// </summary>
public sealed class CycleCountVarianceRecordedConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public CycleCountVarianceRecordedConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "CycleCountVarianceRecorded";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize CycleCountVarianceRecorded payload for event {eventId}.");

        var entry = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            payload.VarianceQuantity,
            $"Cycle count {payload.CycleCountId}");

        await _ledgerRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid CycleCountId,
        Guid CompanyId,
        Guid ProductId,
        Guid WarehouseId,
        decimal VarianceQuantity);
}
