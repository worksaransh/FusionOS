using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetDepreciationSchedule;

public sealed record GetDepreciationScheduleQuery(Guid CompanyId, Guid FixedAssetId, DateTimeOffset AsOfDate)
    : IQuery<DepreciationScheduleDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.read" };
}
