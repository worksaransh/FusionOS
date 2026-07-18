using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;

namespace FusionOS.Modules.Finance.Application.CostCenters.Queries.ListCostCenters;

public sealed record ListCostCentersQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CostCenterDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.cost-center.read" };
}
