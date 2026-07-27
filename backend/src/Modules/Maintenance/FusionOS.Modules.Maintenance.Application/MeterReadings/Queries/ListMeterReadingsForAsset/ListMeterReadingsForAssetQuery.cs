using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.ListMeterReadingsForAsset;

public sealed record ListMeterReadingsForAssetQuery(Guid CompanyId, Guid AssetId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<MeterReadingDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.meter-reading.read" };
}
