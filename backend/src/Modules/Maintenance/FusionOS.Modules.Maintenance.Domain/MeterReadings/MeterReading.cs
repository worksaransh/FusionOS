using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.MeterReadings.Events;

namespace FusionOS.Modules.Maintenance.Domain.MeterReadings;

/// <summary>
/// A single usage/meter reading captured against an Asset — hours run, cycles,
/// mileage, whatever unit that machine's meter reports. This is an independent
/// capture capability only: it does NOT wire into MaintenanceSchedule's due-date
/// trigger — MaintenanceScheduleFrequency.cs's doc comment already documents
/// that usage/meter-based scheduling was explicitly rejected for that slice, and
/// re-opening that decision is a separate, larger follow-up. AssetId is a real,
/// same-module foreign key (Asset lives in this module), validated by the
/// command handler via IAssetRepository — same convention as
/// MaintenanceRequest.AssetId / MaintenanceSchedule.AssetId.
///
/// Unit is a free-text string, not a fixed enum — different assets use
/// different units (hours, cycles, km, …) and there is no shared vocabulary
/// across this module to constrain it to yet; forcing an enum now would either
/// be incomplete or block on units nobody asked for.
/// </summary>
public sealed class MeterReading : TenantAggregateRoot
{
    public Guid AssetId { get; private set; }
    public decimal ReadingValue { get; private set; }
    public DateTimeOffset ReadingDate { get; private set; }
    public string Unit { get; private set; } = default!;

    private MeterReading() { }

    /// <summary>
    /// Creates a reading. Only validates ReadingValue >= 0 and that Unit/AssetId are
    /// present — the monotonically-increasing-per-asset business rule (a real meter
    /// generally only counts up) is deliberately NOT enforced here, because checking
    /// it requires querying the asset's latest prior reading, which is a repository
    /// concern. RecordMeterReadingCommandHandler performs that check (via
    /// IMeterReadingRepository.GetLatestForAssetAsync) before calling this factory —
    /// see its doc comment for the full reasoning.
    /// </summary>
    public static MeterReading Create(Guid companyId, Guid assetId, decimal readingValue, DateTimeOffset readingDate, string unit)
    {
        if (assetId == Guid.Empty)
            throw new ArgumentException("Asset id is required.", nameof(assetId));
        if (readingValue < 0)
            throw new ArgumentException("Reading value cannot be negative.", nameof(readingValue));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("A unit (e.g. \"hours\", \"cycles\", \"km\") is required.", nameof(unit));

        var reading = new MeterReading
        {
            CompanyId = companyId,
            AssetId = assetId,
            ReadingValue = readingValue,
            ReadingDate = readingDate,
            Unit = unit.Trim(),
        };

        reading.Raise(new MeterReadingRecorded(reading.Id, companyId, assetId, readingValue));
        return reading;
    }
}
