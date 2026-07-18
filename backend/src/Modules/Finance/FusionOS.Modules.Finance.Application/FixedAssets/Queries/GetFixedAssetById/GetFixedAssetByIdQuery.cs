using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetFixedAssetById;

public sealed record GetFixedAssetByIdQuery(Guid CompanyId, Guid FixedAssetId)
    : IQuery<FixedAssetDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.read" };
}
