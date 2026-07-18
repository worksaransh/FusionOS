using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Quotations.Events;

public sealed record QuotationRejected(Guid QuotationId, Guid CompanyId, Guid CustomerId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
