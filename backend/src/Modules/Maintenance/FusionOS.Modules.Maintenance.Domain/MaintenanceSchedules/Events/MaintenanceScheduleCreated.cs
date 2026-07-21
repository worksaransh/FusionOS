using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules.Events;

/// <summary>Raised on MaintenanceSchedule creation. No consumer this slice — same deliberate restraint as Assets' AssetCreated.</summary>
public sealed record MaintenanceScheduleCreated(Guid MaintenanceScheduleId, Guid CompanyId, Guid AssetId, string Frequency) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
