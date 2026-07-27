using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Returns.Events;

public sealed record SalesReturnRequested(Guid SalesReturnId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
