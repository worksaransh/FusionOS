using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.SupplierContracts.Events;

public sealed record SupplierContractCreated(Guid SupplierContractId, Guid CompanyId, Guid SupplierId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
