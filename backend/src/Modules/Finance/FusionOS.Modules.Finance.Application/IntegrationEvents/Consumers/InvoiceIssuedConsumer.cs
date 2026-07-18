using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.Receivables;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Sales' InvoiceIssued domain event (relayed via the outbox to
/// Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by posting an Accounts Receivable
/// charge — the consumer InvoiceIssued's own doc comment flagged as "not
/// built yet." Defines its own local payload shape rather than referencing
/// Sales' domain event CLR type, same reviewed pattern as every other
/// consumer in this codebase (Inventory's two consumers, Procurement's
/// GoodsReceiptLineReceivedConsumer): a consumer must never take a
/// compile-time dependency on the producing module's internals.
///
/// <b>GL posting (Phase 2 closeout, 2026-07-18):</b> also posts a balanced
/// Debit AR / Credit Sales Revenue JournalEntry when FinanceSettings has both
/// default accounts configured — closing the gap where an issued invoice
/// updated the AR subledger but never touched the General Ledger, so Trial
/// Balance/P&amp;L/Balance Sheet never reflected it. When FinanceSettings is
/// unconfigured (either account still null), this silently falls back to the
/// pre-existing subledger-only behavior — a consumer has no user-facing
/// channel to surface a "please configure Finance settings" validation error
/// the way a command handler does, so silently skipping is the only sound
/// option, not a shortcut.
/// </summary>
public sealed class InvoiceIssuedConsumer : IIntegrationEventConsumer
{
    private readonly IArLedgerRepository _ledgerRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IFinanceSettingsRepository _settingsRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceIssuedConsumer(
        IArLedgerRepository ledgerRepository,
        IJournalEntryRepository journalEntryRepository,
        IFinanceSettingsRepository settingsRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _journalEntryRepository = journalEntryRepository;
        _settingsRepository = settingsRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "InvoiceIssued";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize InvoiceIssued payload for event {eventId}.");

        var entry = ArLedgerEntry.RecordInvoiceCharge(payload.CompanyId, payload.CustomerId, payload.InvoiceId, payload.TotalAmount);
        await _ledgerRepository.AddAsync(entry, cancellationToken);

        var settings = await _settingsRepository.GetByCompanyIdAsync(payload.CompanyId, cancellationToken);
        if (settings?.DefaultArAccountId is { } arAccountId && settings.DefaultSalesRevenueAccountId is { } revenueAccountId)
        {
            var reference = $"Invoice {payload.InvoiceId} issued";
            var lines = new[]
            {
                new JournalEntryLineInput(arAccountId, payload.TotalAmount, 0m, reference),
                new JournalEntryLineInput(revenueAccountId, 0m, payload.TotalAmount, reference),
            };
            var journalEntry = JournalEntry.Create(payload.CompanyId, reference, lines);
            journalEntry.Post();
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry, the optional journal entry,
        // and the idempotency marker share the same scoped DbContext instance,
        // so this commits all of them atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid InvoiceId, Guid CompanyId, Guid SalesOrderId, Guid CustomerId, decimal TotalAmount);
}
