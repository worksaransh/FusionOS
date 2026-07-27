using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;

namespace FusionOS.Modules.Quality.Application.Calibrations.Queries.ListCalibrationRecords;

/// <summary>OverdueOnly=true scopes to equipment whose NextCalibrationDueDate is before now.</summary>
public sealed record ListCalibrationRecordsQuery(Guid CompanyId, bool OverdueOnly = false, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CalibrationRecordDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.calibration.read" };
}
