using System.Text.Json;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Procurement's PurchaseOrderGoodsReceiptCosted domain event
/// (relayed via the outbox to Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by
/// posting an Accounts Payable charge automatically — the auto-charge follow-up
/// ApLedgerEntry's class doc comment flagged as blocked on a supplier-resolution
/// decision (Phase M8c, 2026-07-17). That decision is now made: Procurement
/// enriches and re-publishes a supplier-attributed event instead of Finance
/// needing a cross-module lookup back into PurchaseOrder. Defines its own
/// local payload shape rather than referencing Procurement's domain event CLR
/// type, same reviewed pattern as every other consumer in this codebase.
/// This does not replace manual RecordBillChargeCommand for ad-hoc bills with
/// no purchase order, or receipts Warehouse posted with no UnitCost — both
/// remain manual, exactly as before this consumer existed.
/// </summary>
public sealed class PurchaseOrderGoodsReceiptCostedConsumer : IIntegrationEventConsumer
{
    private readonly IApLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderGoodsReceiptCostedConsumer(
        IApLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "PurchaseOrderGoodsReceiptCosted";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize PurchaseOrderGoodsReceiptCosted payload for event {eventId}.");

        var description = $"Auto-charge from goods receipt against PO {payload.PurchaseOrderId} ({payload.QuantityReceived} @ {payload.UnitCost})";
        var entry = ApLedgerEntry.RecordBillCharge(payload.CompanyId, payload.SupplierId, payload.PurchaseOrderId, payload.LineAmount, description);

        await _ledgerRepository.AddAsync(entry, cancellationToken);
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry and the idempotency marker
        // share the same scoped DbContext instance behind IApLedgerRepository
        // and IProcessedIntegrationEventStore, so this commits both atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid PurchaseOrderId,
        Guid CompanyId,
        Guid SupplierId,
        Guid ProductId,
        decimal QuantityReceived,
        decimal UnitCost,
        decimal LineAmount);
}
