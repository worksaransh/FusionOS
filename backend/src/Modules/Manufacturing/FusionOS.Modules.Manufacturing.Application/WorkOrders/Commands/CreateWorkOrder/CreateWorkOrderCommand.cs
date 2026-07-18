using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(Guid CompanyId, Guid BillOfMaterialsId, Guid WarehouseId, decimal QuantityToProduce)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.create" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
