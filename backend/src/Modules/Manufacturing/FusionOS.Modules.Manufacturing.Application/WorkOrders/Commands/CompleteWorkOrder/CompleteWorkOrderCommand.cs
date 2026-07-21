using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrder;

public sealed record CompleteWorkOrderCommand(Guid CompanyId, Guid WorkOrderId, decimal? QuantityGoodProduced = null, decimal? QuantityScrapped = null)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.complete" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => WorkOrderId;
    public string Action => "Completed";
}
