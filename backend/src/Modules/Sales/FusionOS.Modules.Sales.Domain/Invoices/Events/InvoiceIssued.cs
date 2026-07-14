using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Invoices.Events;

/// <summary>
/// Matches "SalesInvoiceIssued" as a candidate cross-module integration event
/// (03_SYSTEM_ARCHITECTURE.md §4.2) — Finance's Accounts Receivable slice (not
/// built yet, reserved for Phase 2) would consume this to post a customer
/// receivable. No consumer exists yet, same documented gap as
/// GoodsReceiptLineReceived and JournalEntryPosted.
/// </summary>
public sealed record InvoiceIssued(Guid InvoiceId, Guid CompanyId, Guid SalesOrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
