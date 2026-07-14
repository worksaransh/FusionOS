using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Suppliers.Events;

public sealed record SupplierCreated(Guid SupplierId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
