using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.ListWorkOrders;

public sealed record ListWorkOrdersQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<WorkOrderDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.read" };
}
