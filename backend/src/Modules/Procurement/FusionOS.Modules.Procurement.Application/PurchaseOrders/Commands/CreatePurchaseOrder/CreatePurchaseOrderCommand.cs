using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderCommand(Guid CompanyId, Guid SupplierId, IReadOnlyList<PurchaseOrderLineInput> Lines)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.create" };
    public string EntityType => nameof(Domain.PurchaseOrders.PurchaseOrder);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
