using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Invoices.Events;

public sealed record InvoiceCreated(Guid InvoiceId, Guid CompanyId, Guid SalesOrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
