using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrderForApproval;

/// <summary>
/// Submits a Draft purchase order into Core's generic Approval workflow
/// engine (2026-07-22) — an additive alternative alongside the pre-existing
/// direct ApprovePurchaseOrderCommand, not a replacement for it. Uses the
/// permission code already pre-reserved for this in PermissionCatalog.cs.
/// </summary>
public sealed record SubmitPurchaseOrderForApprovalCommand(Guid CompanyId, Guid PurchaseOrderId)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.submit-for-approval" };
    public string EntityType => nameof(Domain.PurchaseOrders.PurchaseOrder);
    public Guid EntityId => PurchaseOrderId;
    public string Action => "SubmittedForApproval";
}
