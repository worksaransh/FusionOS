namespace FusionOS.Modules.Quality.Application.Calibrations.Contracts;

public sealed record CalibrationRecordDto(
    Guid Id,
    Guid EquipmentReferenceId,
    string EquipmentName,
    string? EquipmentIdentifier,
    string? CalibrationStandard,
    DateTimeOffset LastCalibratedDate,
    DateTimeOffset NextCalibrationDueDate,
    string Result,
    Guid? PerformedByUserId,
    string? PerformedByExternalVendor,
    string? Notes,
    bool IsOverdue);

/// <summary>Single place that turns a CalibrationRecord aggregate into its DTO, shared by every handler that returns one.</summary>
public static class CalibrationRecordMapper
{
    public static CalibrationRecordDto ToDto(Domain.Calibrations.CalibrationRecord record) => new(
        record.Id,
        record.EquipmentReferenceId,
        record.EquipmentName,
        record.EquipmentIdentifier,
        record.CalibrationStandard,
        record.LastCalibratedDate,
        record.NextCalibrationDueDate,
        record.Result.ToString(),
        record.PerformedByUserId,
        record.PerformedByExternalVendor,
        record.Notes,
        IsOverdue: record.NextCalibrationDueDate < DateTimeOffset.UtcNow);
}
