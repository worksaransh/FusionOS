using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Domain.Calibrations;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.RecordCalibrationEvent;

/// <summary>Records a new calibration event against an existing record, overwriting its current-state fields — see CalibrationRecord's doc comment for why this overwrites rather than appending history.</summary>
public sealed record RecordCalibrationEventCommand(
    Guid CompanyId,
    Guid CalibrationRecordId,
    DateTimeOffset CalibratedDate,
    DateTimeOffset NextDueDate,
    CalibrationResult Result,
    Guid? PerformedByUserId,
    string? PerformedByExternalVendor,
    string? Notes)
    : ICommand<CalibrationRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.calibration.record" };
    public string EntityType => nameof(Domain.Calibrations.CalibrationRecord);
    public Guid EntityId => CalibrationRecordId;
    public string Action => "CalibrationRecorded";
}
