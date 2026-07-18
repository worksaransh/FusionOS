using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkOrderById;

public sealed record GetWorkOrderByIdQuery(Guid CompanyId, Guid WorkOrderId)
    : IQuery<WorkOrderDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.read" };
}
