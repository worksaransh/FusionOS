using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.IssueMaterialToWorkOrder;

public sealed record IssueMaterialToWorkOrderCommand(Guid CompanyId, Guid WorkOrderId, Guid ComponentProductId, decimal Quantity)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.issue-material" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => WorkOrderId;
    public string Action => "MaterialIssued";
}
