using System.Text.Json;
using FusionOS.Modules.Ai.Application.DemandHistory.Contracts;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.DemandHistory;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Ai.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Sales' DispatchLineDispatched domain event (relayed via the outbox
/// to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by recording one
/// <see cref="DemandHistoryEntry"/> per dispatched line — real, already-happened
/// demand for a product, the raw material <c>GetDemandForecastQuery</c> computes
/// its linear-regression/moving-average projections from. Same local-payload-shape
/// and idempotency-via-ProcessedIntegrationEvent pattern as Inventory's own
/// DispatchLineDispatchedConsumer (which debits the Stock Ledger from the same
/// event) — a consumer must never take a compile-time dependency on the producing
/// module's internals, only on the documented wire shape.
/// </summary>
public sealed class DispatchLineDispatchedConsumer : IIntegrationEventConsumer
{
    private readonly IDemandHistoryRepository _demandHistoryRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public DispatchLineDispatchedConsumer(
        IDemandHistoryRepository demandHistoryRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _demandHistoryRepository = demandHistoryRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "DispatchLineDispatched";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize DispatchLineDispatched payload for event {eventId}.");

        var entry = DemandHistoryEntry.Record(
            payload.CompanyId,
            payload.ProductId,
            payload.WarehouseId,
            payload.QuantityDispatched,
            payload.OccurredOn);

        await _demandHistoryRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync commits the demand history row and the
        // idempotency marker atomically — they share the same scoped DbContext.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// OccurredOn is carried on the outbox envelope for every domain event (IDomainEvent),
    /// so it is included here even though DispatchLineDispatched's own record doesn't declare
    /// it as a constructor parameter — the outbox relay serializes the full event including
    /// the interface-provided OccurredOn property, matching the wire JSON.
    /// </summary>
    private sealed record Payload(
        Guid DispatchId,
        Guid CompanyId,
        Guid SalesOrderId,
        Guid ProductId,
        Guid WarehouseId,
        decimal QuantityDispatched,
        DateTimeOffset OccurredOn);
}
