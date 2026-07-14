using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

public sealed record PurchaseOrderCreated(Guid PurchaseOrderId, Guid CompanyId, Guid SupplierId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
