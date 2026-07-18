using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReleaseWorkOrder;

public sealed record ReleaseWorkOrderCommand(Guid CompanyId, Guid WorkOrderId)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.release" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => WorkOrderId;
    public string Action => "Released";
}
