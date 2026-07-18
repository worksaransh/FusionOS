using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Quotations.Events;

public sealed record QuotationAccepted(Guid QuotationId, Guid CompanyId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
