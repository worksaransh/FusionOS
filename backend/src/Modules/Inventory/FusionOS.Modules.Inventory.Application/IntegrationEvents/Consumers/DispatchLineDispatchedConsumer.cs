using System.Text.Json;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Sales's DispatchLineDispatched domain event, symmetric to
/// GoodsReceiptLineReceivedConsumer but debiting the Stock Ledger (negative
/// quantity delta) instead of crediting it. Same local-payload-shape and
/// idempotency-via-ProcessedIntegrationEvent pattern — see the doc comment on
/// GoodsReceiptLineReceivedConsumer for the full rationale.
///
/// Not implemented here (documented follow-up): Warehouse's own physical
/// pick/pack/ship execution tracking reacting to this same event.
/// </summary>
public sealed class DispatchLineDispatchedConsumer : IIntegrationEventConsumer
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public DispatchLineDispatchedConsumer(
        IInventoryLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "DispatchLineDispatched";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize DispatchLineDispatched payload for event {eventId}.");

        var entry = InventoryLedgerEntry.RecordAdjustment(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            -payload.QuantityDispatched,
            $"Dispatch {payload.DispatchId}");

        await _ledgerRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid DispatchId,
        Guid CompanyId,
        Guid SalesOrderId,
        Guid ProductId,
        Guid WarehouseId,
        decimal QuantityDispatched);
}
