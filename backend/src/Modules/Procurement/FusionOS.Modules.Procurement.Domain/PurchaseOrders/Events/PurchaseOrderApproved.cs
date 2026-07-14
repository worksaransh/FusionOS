using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

/// <summary>Matches the "PurchaseOrderApproved.v1" event named in 03_SYSTEM_ARCHITECTURE.md §4.2's event catalog.</summary>
public sealed record PurchaseOrderApproved(Guid PurchaseOrderId, Guid CompanyId, Guid SupplierId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
