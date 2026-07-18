using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.CreditNotes.Events;

/// <summary>
/// Candidate cross-module integration event (03_SYSTEM_ARCHITECTURE.md §4.2) —
/// Finance's Accounts Receivable slice consumes this (see
/// FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers.CreditNoteIssuedConsumer)
/// to post a negative AR ledger entry against the customer's balance, mirroring
/// how InvoiceIssued is consumed to post a positive charge.
/// </summary>
public sealed record CreditNoteIssued(Guid CreditNoteId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
