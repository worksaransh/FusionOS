using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

/// <summary>
/// Raised by PurchaseOrder.SubmitForApproval() (2026-07-22) — relayed via the
/// generic outbox to Kafka exactly like PurchaseOrderApproved, so Core's
/// PurchaseOrderSubmittedForApprovalConsumer can create a real ApprovalRequest
/// against this (EntityType="PurchaseOrder", EntityId=PurchaseOrderId) pair
/// without Procurement ever referencing Core's Domain/Application types
/// (03_SYSTEM_ARCHITECTURE.md §2 module boundary).
/// </summary>
public sealed record PurchaseOrderSubmittedForApproval(
    Guid PurchaseOrderId,
    Guid CompanyId,
    Guid RequestedByUserId,
    decimal Amount,
    string Description) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
