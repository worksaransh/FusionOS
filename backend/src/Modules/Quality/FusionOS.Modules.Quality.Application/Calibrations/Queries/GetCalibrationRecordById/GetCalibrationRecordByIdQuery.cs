using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;

namespace FusionOS.Modules.Quality.Application.Calibrations.Queries.GetCalibrationRecordById;

public sealed record GetCalibrationRecordByIdQuery(Guid CompanyId, Guid CalibrationRecordId) : IQuery<CalibrationRecordDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.calibration.read" };
}
