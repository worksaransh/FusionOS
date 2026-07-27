using System.Text.Json;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Quality.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Manufacturing's WorkOrderCompleted domain event (relayed via the outbox to Kafka —
/// 03_SYSTEM_ARCHITECTURE.md §4.2) by auto-opening a Pending Production Inspection against the
/// completed work order, same reactive-instead-of-purely-manual purpose as this module's
/// GoodsReceiptLineReceivedConsumer — see that class's doc comment for the full
/// notification-only rationale (nothing here blocks the work order completion that already
/// happened, and Quality Holds are deliberately not auto-triggered either).
///
/// <see cref="Inspection.ReferenceId"/> is the event's WorkOrderId, matching Inspection's own
/// class doc comment ("Production inspects a Manufacturing Work Order's output").
///
/// Defines its own local payload shape rather than referencing Manufacturing's domain event CLR
/// type, same reviewed pattern as every other consumer in this codebase.
/// </summary>
public sealed class WorkOrderCompletedConsumer : IIntegrationEventConsumer
{
    private static readonly IReadOnlyCollection<string> DefaultCharacteristics = new[] { "Visual inspection" };

    private readonly IInspectionRepository _repository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public WorkOrderCompletedConsumer(
        IInspectionRepository repository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
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
        ValidatePayload(payload, eventId, companyId);

        var inspection = Inspection.Create(payload.CompanyId, InspectionType.Production, payload.WorkOrderId, DefaultCharacteristics);
        await _repository.AddAsync(inspection, cancellationToken);

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync commits the new Inspection and the idempotency marker
        // atomically — they share the same scoped DbContext instance.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePayload(Payload payload, Guid eventId, Guid envelopeCompanyId)
    {
        if (payload.WorkOrderId == Guid.Empty)
            throw new InvalidOperationException($"WorkOrderCompleted payload for event {eventId} has an empty WorkOrderId.");
        if (payload.CompanyId == Guid.Empty)
            throw new InvalidOperationException($"WorkOrderCompleted payload for event {eventId} has an empty CompanyId.");
        // Bus-routing bugs could in principle deliver a message whose envelope companyId
        // doesn't match the payload's own CompanyId — never trust the payload alone to
        // decide which tenant's Inspection gets created.
        if (payload.CompanyId != envelopeCompanyId)
            throw new InvalidOperationException($"WorkOrderCompleted payload for event {eventId} has CompanyId {payload.CompanyId} which does not match the envelope's companyId {envelopeCompanyId}.");
    }

    private sealed record Payload(
        Guid WorkOrderId,
        Guid CompanyId,
        Guid WarehouseId,
        Guid ProductId,
        decimal QuantityProduced,
        IReadOnlyList<ComponentPayload>? Components,
        decimal QuantityScrapped,
        decimal YieldPercentage);

    private sealed record ComponentPayload(Guid ComponentProductId, decimal QuantityConsumed);
}
