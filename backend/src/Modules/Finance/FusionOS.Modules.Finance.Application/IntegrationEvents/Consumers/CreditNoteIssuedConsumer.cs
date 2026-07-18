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
/// Reacts to Sales' CreditNoteIssued domain event (relayed via the outbox to
/// Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) by posting a negative Accounts
/// Receivable entry against the customer's balance — mirrors
/// InvoiceIssuedConsumer exactly, defining its own local payload shape rather
/// than referencing Sales' domain event CLR type. Does not re-validate
/// business rules already checked at write time by CreateCreditNoteCommandHandler
/// (e.g. that the credited quantity doesn't exceed what was invoiced) — same
/// restraint InvoiceIssuedConsumer takes with invoice quantities.
///
/// <b>GL posting (Phase 2 closeout, 2026-07-18):</b> also posts the mirror
/// image of InvoiceIssuedConsumer's journal entry — Debit Sales Revenue /
/// Credit AR, reversing part of the original sale — when FinanceSettings has
/// both accounts configured. Same silent-skip-when-unconfigured restraint as
/// InvoiceIssuedConsumer (see its own doc comment for why).
/// </summary>
public sealed class CreditNoteIssuedConsumer : IIntegrationEventConsumer
{
    private readonly IArLedgerRepository _ledgerRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IFinanceSettingsRepository _settingsRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public CreditNoteIssuedConsumer(
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

    public string EventType => "CreditNoteIssued";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize CreditNoteIssued payload for event {eventId}.");

        var entry = ArLedgerEntry.RecordCreditNote(payload.CompanyId, payload.CustomerId, payload.InvoiceId, payload.CreditNoteId, payload.TotalAmount);
        await _ledgerRepository.AddAsync(entry, cancellationToken);

        var settings = await _settingsRepository.GetByCompanyIdAsync(payload.CompanyId, cancellationToken);
        if (settings?.DefaultArAccountId is { } arAccountId && settings.DefaultSalesRevenueAccountId is { } revenueAccountId)
        {
            var reference = $"Credit note {payload.CreditNoteId} issued against invoice {payload.InvoiceId}";
            var lines = new[]
            {
                new JournalEntryLineInput(revenueAccountId, payload.TotalAmount, 0m, reference),
                new JournalEntryLineInput(arAccountId, 0m, payload.TotalAmount, reference),
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

    private sealed record Payload(Guid CreditNoteId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, decimal TotalAmount);
}
