using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Activities.Events;

/// <summary>Raised when an interaction is logged. No consumer today — a natural future hook for activity-feed/notification fan-out.</summary>
public sealed record ActivityLogged(Guid ActivityId, Guid CompanyId, string EntityType, Guid EntityId, ActivityType Type) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
