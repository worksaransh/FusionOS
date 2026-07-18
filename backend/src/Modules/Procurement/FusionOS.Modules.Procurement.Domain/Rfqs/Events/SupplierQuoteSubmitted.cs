using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Rfqs.Events;

public sealed record SupplierQuoteSubmitted(Guid RfqId, Guid CompanyId, Guid SupplierQuoteId, Guid SupplierId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
