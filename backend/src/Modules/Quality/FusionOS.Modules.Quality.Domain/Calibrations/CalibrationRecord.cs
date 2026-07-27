using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.Calibrations;

/// <summary>
/// Tracks calibration for one piece of measurement/test equipment. <see cref="EquipmentReferenceId"/>
/// is an opaque cross-module reference — it may point at a Maintenance.Asset, but it is never
/// existence-validated or FK'd here (03_SYSTEM_ARCHITECTURE.md §2 — no cross-module FK or
/// query-time dependency; same convention as every other cross-module Guid in this codebase,
/// e.g. Inspection.ReferenceId). <see cref="EquipmentName"/>/<see cref="EquipmentIdentifier"/>
/// make this record self-describing even without ever resolving that opaque id — Maintenance
/// may not even be installed for a given tenant, and calibration tracking (test gauges,
/// scales, calipers, thermometers) is a Quality concept in its own right, not necessarily 1:1
/// with a Maintenance Asset.
///
/// <b>Scope decision — current-state, not a history log:</b> this is one row per piece of
/// equipment, holding its LATEST calibration state (matches the "current-state master data"
/// pattern Maintenance's own Asset uses — Code/Name/IsActive, no sub-collection of every past
/// change). <see cref="RecordCalibration"/> therefore OVERWRITES LastCalibratedDate/
/// NextCalibrationDueDate/Result/PerformedBy*/Notes on every call rather than appending a new
/// history row. A full calibration HISTORY log (one immutable row per calibration event, so
/// "show me every time this gauge was calibrated" is answerable) is a reasonable, separately-
/// scoped follow-up — this slice only needs "is this equipment currently in/out of
/// calibration," which the current-state shape answers directly via NextCalibrationDueDate.
/// </summary>
public sealed class CalibrationRecord : TenantAggregateRoot
{
    public Guid EquipmentReferenceId { get; private set; }
    public string EquipmentName { get; private set; } = default!;
    public string? EquipmentIdentifier { get; private set; }
    public string? CalibrationStandard { get; private set; }
    public DateTimeOffset LastCalibratedDate { get; private set; }
    public DateTimeOffset NextCalibrationDueDate { get; private set; }
    public CalibrationResult Result { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public string? PerformedByExternalVendor { get; private set; }
    public string? Notes { get; private set; }

    private CalibrationRecord() { }

    /// <summary>Registers a new piece of equipment into calibration tracking, with its first calibration event.</summary>
    public static CalibrationRecord Create(
        Guid companyId,
        Guid equipmentReferenceId,
        string equipmentName,
        string? equipmentIdentifier,
        string? calibrationStandard,
        DateTimeOffset lastCalibratedDate,
        DateTimeOffset nextCalibrationDueDate,
        CalibrationResult result,
        Guid? performedByUserId,
        string? performedByExternalVendor,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(equipmentName))
            throw new ArgumentException("Equipment name is required.", nameof(equipmentName));
        if (nextCalibrationDueDate < lastCalibratedDate)
            throw new ArgumentException("Next calibration due date cannot be before the last calibrated date.", nameof(nextCalibrationDueDate));

        return new CalibrationRecord
        {
            CompanyId = companyId,
            EquipmentReferenceId = equipmentReferenceId,
            EquipmentName = equipmentName.Trim(),
            EquipmentIdentifier = string.IsNullOrWhiteSpace(equipmentIdentifier) ? null : equipmentIdentifier.Trim(),
            CalibrationStandard = string.IsNullOrWhiteSpace(calibrationStandard) ? null : calibrationStandard.Trim(),
            LastCalibratedDate = lastCalibratedDate,
            NextCalibrationDueDate = nextCalibrationDueDate,
            Result = result,
            PerformedByUserId = performedByUserId,
            PerformedByExternalVendor = string.IsNullOrWhiteSpace(performedByExternalVendor) ? null : performedByExternalVendor.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }

    /// <summary>
    /// Records a new calibration event, overwriting the current-state fields (see this
    /// class's doc comment for why this overwrites rather than appends to a history
    /// collection).
    /// </summary>
    public void RecordCalibration(
        DateTimeOffset calibratedDate,
        DateTimeOffset nextDueDate,
        CalibrationResult result,
        Guid? performedByUserId,
        string? performedByExternalVendor,
        string? notes)
    {
        if (nextDueDate < calibratedDate)
            throw new ArgumentException("Next calibration due date cannot be before the calibrated date.", nameof(nextDueDate));

        LastCalibratedDate = calibratedDate;
        NextCalibrationDueDate = nextDueDate;
        Result = result;
        PerformedByUserId = performedByUserId;
        PerformedByExternalVendor = string.IsNullOrWhiteSpace(performedByExternalVendor) ? null : performedByExternalVendor.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
