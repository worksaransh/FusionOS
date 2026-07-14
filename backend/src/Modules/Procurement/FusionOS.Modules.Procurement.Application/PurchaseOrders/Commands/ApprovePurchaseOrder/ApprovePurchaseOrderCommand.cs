using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed record ApprovePurchaseOrderCommand(Guid CompanyId, Guid PurchaseOrderId)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.approve" };
    public string EntityType => nameof(Domain.PurchaseOrders.PurchaseOrder);
    public Guid EntityId => PurchaseOrderId;
    public string Action => "Approved";
}
