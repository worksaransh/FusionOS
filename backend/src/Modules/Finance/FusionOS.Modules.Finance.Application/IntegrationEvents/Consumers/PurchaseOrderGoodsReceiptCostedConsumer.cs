using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
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
///
/// <b>GL posting (Phase 2 closeout, 2026-07-18):</b> also posts a balanced
/// Debit Purchase Expense / Credit AP JournalEntry when FinanceSettings has
/// both default accounts configured — same silent-skip-when-unconfigured
/// restraint as InvoiceIssuedConsumer (see its own doc comment for why).
///
/// <b>Three-way match (2026-07-20):</b> also accumulates the receipt's
/// LineAmount into the local PurchaseOrderFact read-model — the received-amount
/// leg RecordBillChargeCommandHandler validates manual bills against (see
/// PurchaseOrderFact's class doc comment for the full policy). The auto-charge
/// this consumer itself posts is deliberately NOT match-validated: it records
/// exactly what was received at the cost it was received at (billed == received
/// by construction), and a Kafka consumer has no user-facing channel to surface
/// a rejection anyway — an over-receipt against the ordered amount is a
/// Procurement/Warehouse problem to surface, not a message to poison.
/// </summary>
public sealed class PurchaseOrderGoodsReceiptCostedConsumer : IIntegrationEventConsumer
{
    private readonly IApLedgerRepository _ledgerRepository;
    private readonly IPurchaseOrderFactRepository _factRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IFinanceSettingsRepository _settingsRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderGoodsReceiptCostedConsumer(
        IApLedgerRepository ledgerRepository,
        IPurchaseOrderFactRepository factRepository,
        IJournalEntryRepository journalEntryRepository,
        IFinanceSettingsRepository settingsRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _factRepository = factRepository;
        _journalEntryRepository = journalEntryRepository;
        _settingsRepository = settingsRepository;
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
        ValidatePayload(payload, eventId);

        var description = $"Auto-charge from goods receipt against PO {payload.PurchaseOrderId} ({payload.QuantityReceived} @ {payload.UnitCost})";
        var entry = ApLedgerEntry.RecordBillCharge(payload.CompanyId, payload.SupplierId, payload.PurchaseOrderId, payload.LineAmount, description);
        await _ledgerRepository.AddAsync(entry, cancellationToken);

        // Received-amount leg of the three-way match (see PurchaseOrderFact's
        // class doc comment): accumulate this costed receipt into the local
        // fact, creating it with OrderedAmount = null when this event beat the
        // PurchaseOrderApproved event here (separate topics, no cross-topic
        // ordering guarantee — PurchaseOrderApprovedConsumer fills it in later).
        var fact = await _factRepository.GetByPurchaseOrderIdAsync(payload.CompanyId, payload.PurchaseOrderId, cancellationToken);
        if (fact is null)
        {
            fact = PurchaseOrderFact.FromGoodsReceipt(payload.CompanyId, payload.PurchaseOrderId, payload.SupplierId, payload.LineAmount);
            await _factRepository.AddAsync(fact, cancellationToken);
        }
        else
        {
            fact.ApplyGoodsReceipt(payload.LineAmount);
        }

        var settings = await _settingsRepository.GetByCompanyIdAsync(payload.CompanyId, cancellationToken);
        if (settings?.DefaultPurchaseExpenseAccountId is { } expenseAccountId && settings.DefaultApAccountId is { } apAccountId)
        {
            var lines = new[]
            {
                new JournalEntryLineInput(expenseAccountId, payload.LineAmount, 0m, description),
                new JournalEntryLineInput(apAccountId, 0m, payload.LineAmount, description),
            };
            var journalEntry = JournalEntry.Create(payload.CompanyId, description, lines);
            journalEntry.Post();
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry, the fact upsert, the
        // optional journal entry, and the idempotency marker share the same
        // scoped DbContext instance, so this commits all of them atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Guards against a malformed-but-parseable payload silently proceeding
    /// with garbage data (e.g. System.Text.Json defaulting a missing/mistyped
    /// property to <see cref="Guid.Empty"/> or 0 rather than throwing) — an
    /// ApLedgerEntry/PurchaseOrderFact keyed on Guid.Empty, or a negative
    /// LineAmount posted as a real AP charge, would otherwise get created
    /// silently. Throwing here hands the message to
    /// KafkaConsumerHostedService's bounded retry-then-give-up dispatch loop
    /// instead of letting it corrupt the ledger/read model.
    /// </summary>
    private static void ValidatePayload(Payload payload, Guid eventId)
    {
        if (payload.PurchaseOrderId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderGoodsReceiptCosted payload for event {eventId} has an empty PurchaseOrderId.");
        if (payload.CompanyId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderGoodsReceiptCosted payload for event {eventId} has an empty CompanyId.");
        if (payload.SupplierId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderGoodsReceiptCosted payload for event {eventId} has an empty SupplierId.");
        if (payload.LineAmount < 0m)
            throw new InvalidOperationException($"PurchaseOrderGoodsReceiptCosted payload for event {eventId} has a negative LineAmount ({payload.LineAmount}).");
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
