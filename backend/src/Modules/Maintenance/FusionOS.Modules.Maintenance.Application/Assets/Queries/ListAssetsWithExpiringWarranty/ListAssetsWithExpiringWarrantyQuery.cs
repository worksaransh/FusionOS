using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.ListAssetsWithExpiringWarranty;

/// <summary>Assets whose warranty expires within the next <paramref name="WithinDays"/> days (or has already expired) — reuses maintenance.asset.read, same permission as every other Asset read query.</summary>
public sealed record ListAssetsWithExpiringWarrantyQuery(Guid CompanyId, int WithinDays = 30) : IQuery<IReadOnlyList<AssetDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.read" };
}
