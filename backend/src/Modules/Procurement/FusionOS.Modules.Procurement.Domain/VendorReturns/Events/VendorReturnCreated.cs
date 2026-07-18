using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.VendorReturns.Events;

public sealed record VendorReturnCreated(Guid VendorReturnId, Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, decimal Quantity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
