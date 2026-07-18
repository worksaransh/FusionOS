using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.ListAssets;

public sealed record ListAssetsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AssetDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.read" };
}
