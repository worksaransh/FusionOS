namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;

public sealed record MeterReadingDto(Guid Id, Guid AssetId, decimal ReadingValue, DateTimeOffset ReadingDate, string Unit, DateTimeOffset CreatedAt);

/// <summary>Single place that turns a MeterReading aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MeterReadingMapper
{
    public static MeterReadingDto ToDto(Domain.MeterReadings.MeterReading reading) => new(
        reading.Id,
        reading.AssetId,
        reading.ReadingValue,
        reading.ReadingDate,
        reading.Unit,
        reading.CreatedAt);
}
