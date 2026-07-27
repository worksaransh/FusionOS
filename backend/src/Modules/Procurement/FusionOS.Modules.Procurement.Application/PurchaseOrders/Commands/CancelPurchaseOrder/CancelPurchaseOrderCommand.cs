using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(Guid CompanyId, Guid PurchaseOrderId)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.cancel" };
    public string EntityType => nameof(Domain.PurchaseOrders.PurchaseOrder);
    public Guid EntityId => PurchaseOrderId;
    public string Action => "Cancelled";
}
