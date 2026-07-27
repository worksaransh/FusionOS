using System.Text.Json;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Quality.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Warehouse's GoodsReceiptLineReceived domain event (relayed via the outbox to Kafka
/// — 03_SYSTEM_ARCHITECTURE.md §4.2) by auto-opening a Pending IncomingGoods Inspection so a
/// receipt is never missed for inspection — this is what makes Quality reactive instead of
/// purely manual (previously QualityModule's own doc comment noted "Quality... consumes none").
///
/// <b>Notification-only, not a gate:</b> creating this Inspection record does not block, hold,
/// or otherwise slow down the underlying goods receipt in any way — the receipt has already
/// posted by the time this consumer runs. The Inspection starts Pending (uninspected); a real
/// inspector still has to come record actual pass/fail results via RecordInspectionResultsCommand
/// exactly as before this consumer existed. Quality Holds (a separate feature another concurrent
/// agent is building) are deliberately NOT auto-triggered here either — same restraint.
///
/// <see cref="Inspection.ReferenceId"/> is set to the event's GoodsReceiptId, not PurchaseOrderId
/// — Inspection's own class doc comment already documents IncomingGoods.ReferenceId as "a
/// Procurement/Warehouse Goods Receipt id", so this keeps that pre-existing documented contract
/// rather than introducing a second id shape for the same InspectionType.
///
/// Uses a single "Visual inspection" placeholder characteristic since there is no cross-module
/// InspectionPlan lookup available here (Quality.Application cannot take a compile-time
/// dependency on any other module) — if an InspectionPlan for IncomingGoods exists it is a
/// same-module concept another concurrent agent may wire up separately; this consumer
/// deliberately does not require one to exist.
///
/// Defines its own local payload shape rather than referencing Warehouse's domain event CLR
/// type, same reviewed pattern as every other consumer in this codebase (e.g. Finance's
/// InvoiceIssuedConsumer, Inventory's WorkOrderCompletedConsumer).
/// </summary>
public sealed class GoodsReceiptLineReceivedConsumer : IIntegrationEventConsumer
{
    private static readonly IReadOnlyCollection<string> DefaultCharacteristics = new[] { "Visual inspection" };

    private readonly IInspectionRepository _repository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public GoodsReceiptLineReceivedConsumer(
        IInspectionRepository repository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
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
        ValidatePayload(payload, eventId, companyId);

        var inspection = Inspection.Create(payload.CompanyId, InspectionType.IncomingGoods, payload.GoodsReceiptId, DefaultCharacteristics);
        await _repository.AddAsync(inspection, cancellationToken);

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync commits the new Inspection and the idempotency marker
        // atomically — they share the same scoped DbContext instance.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePayload(Payload payload, Guid eventId, Guid envelopeCompanyId)
    {
        if (payload.GoodsReceiptId == Guid.Empty)
            throw new InvalidOperationException($"GoodsReceiptLineReceived payload for event {eventId} has an empty GoodsReceiptId.");
        if (payload.CompanyId == Guid.Empty)
            throw new InvalidOperationException($"GoodsReceiptLineReceived payload for event {eventId} has an empty CompanyId.");
        // Bus-routing bugs could in principle deliver a message whose envelope companyId
        // doesn't match the payload's own CompanyId — never trust the payload alone to
        // decide which tenant's Inspection gets created.
        if (payload.CompanyId != envelopeCompanyId)
            throw new InvalidOperationException($"GoodsReceiptLineReceived payload for event {eventId} has CompanyId {payload.CompanyId} which does not match the envelope's companyId {envelopeCompanyId}.");
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
        string? BatchNumber = null,
        string? SerialNumber = null);
}
