using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Rfqs.Events;

public sealed record RfqAwarded(Guid RfqId, Guid CompanyId, Guid SupplierQuoteId, Guid SupplierId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
