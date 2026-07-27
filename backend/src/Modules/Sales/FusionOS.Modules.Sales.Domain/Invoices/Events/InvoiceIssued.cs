using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Invoices.Events;

/// <summary>
/// Matches "SalesInvoiceIssued" as a candidate cross-module integration event
/// (03_SYSTEM_ARCHITECTURE.md §4.2) — Finance's Accounts Receivable slice
/// consumes this to post a customer receivable (InvoiceIssuedConsumer).
/// </summary>
/// <summary>One TaxRateId's worth of tax across this invoice's lines (same rate on multiple lines is summed) — GST filing support (2026-07-22), consumed by Finance's InvoiceIssuedConsumer to record output TaxTransaction rows.</summary>
public sealed record InvoiceIssuedTaxLine(Guid TaxRateId, decimal TaxAmount);

public sealed record InvoiceIssued(Guid InvoiceId, Guid CompanyId, Guid SalesOrderId, Guid CustomerId, decimal TotalAmount, IReadOnlyList<InvoiceIssuedTaxLine>? TaxLines = null) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
