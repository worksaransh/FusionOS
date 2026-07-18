using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;

namespace FusionOS.Modules.Finance.Application.CostCenters.Queries.GetCostCenterById;

public sealed record GetCostCenterByIdQuery(Guid CompanyId, Guid CostCenterId)
    : IQuery<CostCenterDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.cost-center.read" };
}
