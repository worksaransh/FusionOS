using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.RejectPurchaseOrder;

/// <summary>
/// Rejects a purchase order directly (Phase 10 remediation, 2026-07-27) —
/// an additive, manual counterpart to ApprovalRequestRejectedConsumer's
/// existing internal call into PurchaseOrder.Reject() once Core's Approval
/// workflow engine records a rejected step. This command exposes the same
/// domain method for straightforward manual rejection outside that
/// approval-engine path, e.g. a maker-checker reviewer turning down a Draft
/// PO directly without ever routing it through Core's Approval workflow.
/// </summary>
public sealed record RejectPurchaseOrderCommand(Guid CompanyId, Guid PurchaseOrderId)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.reject" };
    public string EntityType => nameof(Domain.PurchaseOrders.PurchaseOrder);
    public Guid EntityId => PurchaseOrderId;
    public string Action => "Rejected";
}
