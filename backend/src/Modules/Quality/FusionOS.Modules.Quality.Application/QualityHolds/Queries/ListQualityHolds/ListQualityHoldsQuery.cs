using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Queries.ListQualityHolds;

/// <summary>
/// EntityType/EntityId together scope to holds against one specific record (e.g. "what holds
/// exist on InventoryLot X"); either can be omitted independently. ActiveOnly, when true,
/// restricts to Status=Active — "what's currently on hold" across the company.
/// </summary>
public sealed record ListQualityHoldsQuery(Guid CompanyId, string? EntityType = null, Guid? EntityId = null, bool? ActiveOnly = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<QualityHoldDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.quality-hold.read" };
}
