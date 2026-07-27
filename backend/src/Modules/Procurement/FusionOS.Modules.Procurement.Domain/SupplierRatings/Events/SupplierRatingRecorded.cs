using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.SupplierRatings.Events;

public sealed record SupplierRatingRecorded(Guid SupplierRatingId, Guid CompanyId, Guid SupplierId, Guid? PurchaseOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
