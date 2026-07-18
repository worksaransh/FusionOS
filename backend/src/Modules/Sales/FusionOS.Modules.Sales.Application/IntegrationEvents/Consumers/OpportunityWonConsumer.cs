using System.Text.Json;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Domain.Customers;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Sales.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to CRM's OpportunityWon domain event (relayed via the outbox to Kafka —
/// 03_SYSTEM_ARCHITECTURE.md §4.2) by creating the real Sales Customer the won deal
/// implies, via <see cref="Customer.Create"/> — the "reuse Customer.Create as the
/// conversion target" the CRM slice was designed around. CRM never creates a Customer
/// itself; Sales owns Customer and applies it here (same producer/consumer split as
/// WorkOrderCompleted → Inventory).
///
/// Defines its own local payload shape rather than referencing CRM's domain event CLR
/// type — a consumer must never take a compile-time dependency on the producing module's
/// internals. Idempotent twice over: the event-id dedupe guard, plus a
/// customer-code-already-exists check so a redelivery (or a code a user already created by
/// hand) is a no-op rather than a duplicate-key failure.
/// </summary>
public sealed class OpportunityWonConsumer : IIntegrationEventConsumer
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public OpportunityWonConsumer(
        ICustomerRepository customerRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "OpportunityWon";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize OpportunityWon payload for event {eventId}.");

        // Skip creation if a customer with this code already exists — keeps the consumer a
        // no-op on redelivery or when the code was already taken, instead of a unique-key throw.
        if (!await _customerRepository.CodeExistsAsync(payload.CompanyId, payload.CustomerCode, cancellationToken))
        {
            var customer = Customer.Create(payload.CompanyId, payload.CustomerName, payload.CustomerCode, payload.ContactEmail);
            await _customerRepository.AddAsync(customer, cancellationToken);
        }

        _processedEvents.MarkProcessed(eventId, EventType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(
        Guid OpportunityId,
        Guid CompanyId,
        string CustomerName,
        string CustomerCode,
        string? ContactEmail);
}
