using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.MeterReadings.Events;

/// <summary>Raised when a meter/usage reading is recorded. No consumer this slice — same deliberate restraint as AssetCreated.</summary>
public sealed record MeterReadingRecorded(Guid MeterReadingId, Guid CompanyId, Guid AssetId, decimal ReadingValue) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
