using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.GetAssetById;

public sealed record GetAssetByIdQuery(Guid CompanyId, Guid AssetId) : IQuery<AssetDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.read" };
}
