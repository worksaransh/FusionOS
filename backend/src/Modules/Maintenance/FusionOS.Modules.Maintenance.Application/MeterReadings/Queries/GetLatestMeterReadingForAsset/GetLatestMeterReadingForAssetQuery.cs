using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.GetLatestMeterReadingForAsset;

/// <summary>Returns null (not NotFound) when the asset has no readings yet — a freshly registered asset legitimately has no meter history.</summary>
public sealed record GetLatestMeterReadingForAssetQuery(Guid CompanyId, Guid AssetId) : IQuery<MeterReadingDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.meter-reading.read" };
}
