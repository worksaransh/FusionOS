using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Domain.Calibrations;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.CreateCalibrationRecord;

/// <summary>Registers a new piece of equipment into calibration tracking. EquipmentReferenceId is an opaque cross-module reference, never validated — see CalibrationRecord's doc comment.</summary>
public sealed record CreateCalibrationRecordCommand(
    Guid CompanyId,
    Guid EquipmentReferenceId,
    string EquipmentName,
    string? EquipmentIdentifier,
    string? CalibrationStandard,
    DateTimeOffset LastCalibratedDate,
    DateTimeOffset NextCalibrationDueDate,
    CalibrationResult Result,
    Guid? PerformedByUserId,
    string? PerformedByExternalVendor,
    string? Notes)
    : ICommand<CalibrationRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.calibration.create" };
    public string EntityType => nameof(Domain.Calibrations.CalibrationRecord);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
