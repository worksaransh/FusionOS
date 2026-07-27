using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

/// <summary>
/// Raised by PurchaseOrder.Cancel() (Phase 10 remediation, 2026-07-27), same
/// shape/audience as PurchaseOrderRejected — no consumer reacts to it yet
/// anywhere in the repo, but every other terminal PurchaseOrder state change
/// raises an event on the generic outbox and this one shouldn't be a silent
/// exception to that.
/// </summary>
public sealed record PurchaseOrderCancelled(Guid PurchaseOrderId, Guid CompanyId, Guid SupplierId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
