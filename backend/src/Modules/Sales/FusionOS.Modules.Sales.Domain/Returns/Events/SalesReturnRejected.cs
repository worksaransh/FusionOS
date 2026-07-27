using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Returns.Events;

public sealed record SalesReturnRejected(Guid SalesReturnId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, string RejectionReason) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
