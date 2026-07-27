using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.ListWorkCenters;

public sealed record ListWorkCentersQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<WorkCenterDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-center.read" };
}
