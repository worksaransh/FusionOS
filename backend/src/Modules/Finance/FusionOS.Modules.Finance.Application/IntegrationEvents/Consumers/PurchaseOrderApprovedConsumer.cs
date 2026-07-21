using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Procurement's PurchaseOrderApproved domain event (relayed via the
/// outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2, where this exact
/// producer/consumer pair is named in the event catalog: "PurchaseOrderApproved.v1
/// → Finance (AP accrual)") by recording the approved PO's total as a local
/// PurchaseOrderFact — the ordered-amount leg of the three-way match that
/// RecordBillChargeCommandHandler enforces (see PurchaseOrderFact's class doc
/// comment for the full policy). Deliberately does NOT post any ledger or GL
/// entry: approval means "we intend to buy," not "we owe money" — the AP charge
/// itself still comes from PurchaseOrderGoodsReceiptCostedConsumer (goods
/// actually received) or a manual RecordBillChargeCommand.
///
/// Defines its own local payload shape rather than referencing Procurement's
/// domain event CLR type, same reviewed pattern as every other consumer in
/// this codebase.
/// </summary>
public sealed class PurchaseOrderApprovedConsumer : IIntegrationEventConsumer
{
    private readonly IPurchaseOrderFactRepository _factRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderApprovedConsumer(
        IPurchaseOrderFactRepository factRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _factRepository = factRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "PurchaseOrderApproved";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize PurchaseOrderApproved payload for event {eventId}.");
        ValidatePayload(payload, eventId);

        var fact = await _factRepository.GetByPurchaseOrderIdAsync(payload.CompanyId, payload.PurchaseOrderId, cancellationToken);
        if (fact is null)
        {
            fact = PurchaseOrderFact.FromApproval(payload.CompanyId, payload.PurchaseOrderId, payload.SupplierId, payload.TotalAmount);
            await _factRepository.AddAsync(fact, cancellationToken);
        }
        else
        {
            // A costed goods-receipt event beat the approval event here
            // (separate topics, no cross-topic ordering guarantee) and already
            // created the fact with OrderedAmount = null — fill it in now.
            fact.ApplyApproval(payload.TotalAmount);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the fact upsert and the idempotency marker
        // share the same scoped DbContext instance, so this commits both
        // atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Guards against a malformed-but-parseable payload silently proceeding
    /// with garbage data (e.g. System.Text.Json defaulting a missing/mistyped
    /// property to <see cref="Guid.Empty"/> or 0 rather than throwing) — a
    /// PurchaseOrderFact keyed on Guid.Empty would otherwise get created
    /// silently. Throwing here hands the message to
    /// KafkaConsumerHostedService's bounded retry-then-give-up dispatch loop
    /// instead of letting it corrupt the read model.
    /// </summary>
    private static void ValidatePayload(Payload payload, Guid eventId)
    {
        if (payload.PurchaseOrderId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderApproved payload for event {eventId} has an empty PurchaseOrderId.");
        if (payload.CompanyId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderApproved payload for event {eventId} has an empty CompanyId.");
        if (payload.SupplierId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderApproved payload for event {eventId} has an empty SupplierId.");
        if (payload.TotalAmount < 0m)
            throw new InvalidOperationException($"PurchaseOrderApproved payload for event {eventId} has a negative TotalAmount ({payload.TotalAmount}).");
    }

    private sealed record Payload(
        Guid PurchaseOrderId,
        Guid CompanyId,
        Guid SupplierId,
        decimal TotalAmount);
}
