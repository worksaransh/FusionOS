using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

/// <summary>
/// Raised by PurchaseOrder.Reject() (2026-07-22), same shape/audience as
/// PurchaseOrderApproved — no consumer reacts to it yet anywhere in the repo,
/// but every other terminal PurchaseOrder state change raises an event on the
/// generic outbox and this one shouldn't be a silent exception to that.
/// </summary>
public sealed record PurchaseOrderRejected(Guid PurchaseOrderId, Guid CompanyId, Guid SupplierId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
