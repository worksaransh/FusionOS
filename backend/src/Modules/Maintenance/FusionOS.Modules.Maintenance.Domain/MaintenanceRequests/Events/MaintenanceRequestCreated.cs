using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests.Events;

public sealed record MaintenanceRequestCreated(Guid MaintenanceRequestId, Guid CompanyId, Guid AssetId, string Type) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
