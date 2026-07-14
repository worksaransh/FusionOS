using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
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
/// </summary>
public sealed class InvoiceIssuedConsumer : IIntegrationEventConsumer
{
    private readonly IArLedgerRepository _ledgerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceIssuedConsumer(
        IArLedgerRepository ledgerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
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
        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ledger entry and the idempotency marker
        // share the same scoped DbContext instance behind IArLedgerRepository
        // and IProcessedIntegrationEventStore, so this commits both atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid InvoiceId, Guid CompanyId, Guid SalesOrderId, Guid CustomerId, decimal TotalAmount);
}
