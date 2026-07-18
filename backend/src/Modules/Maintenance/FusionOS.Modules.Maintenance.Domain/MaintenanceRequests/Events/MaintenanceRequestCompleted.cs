using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests.Events;

/// <summary>Raised once a maintenance request is resolved. No consumer this slice — the natural future hook for a maintenance-cost/downtime report that doesn't exist yet.</summary>
public sealed record MaintenanceRequestCompleted(Guid MaintenanceRequestId, Guid CompanyId, Guid AssetId, string Type) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
