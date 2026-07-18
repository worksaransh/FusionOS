using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.SupplierContracts.Events;

/// <summary>Raised on termination — no consumer wired this phase, same restraint as RfqConverted/QuotationConverted's unwired state. Documented as the natural future hook for a supplier-relationship-change notification.</summary>
public sealed record SupplierContractTerminated(Guid SupplierContractId, Guid CompanyId, Guid SupplierId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
