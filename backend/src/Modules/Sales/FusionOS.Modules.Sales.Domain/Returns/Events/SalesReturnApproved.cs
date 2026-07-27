using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Returns.Events;

public sealed record SalesReturnApproved(Guid SalesReturnId, Guid CompanyId, Guid InvoiceId, Guid CustomerId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
