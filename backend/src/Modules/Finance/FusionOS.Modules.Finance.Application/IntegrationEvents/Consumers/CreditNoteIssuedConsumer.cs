using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
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
/// </summary>
public sealed class CreditNoteIssuedConsumer : IIntegrationEventConsumer
{
    private readonly IArLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public CreditNoteIssuedConsumer(
        IArLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
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
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry and the idempotency marker
        // share the same scoped DbContext instance behind IArLedgerRepository
        // and IProcessedIntegrationEventStore, so this commits both atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid CreditNoteId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, decimal TotalAmount);
}
