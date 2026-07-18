using FusionOS.SharedKernel;

namespace FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots.Events;

/// <summary>Raised on KpiSnapshot creation. No consumer this slice — the natural future hook for an alerting/threshold-breach follow-up.</summary>
public sealed record KpiSnapshotRecorded(Guid KpiSnapshotId, Guid CompanyId, Guid KpiDefinitionId, decimal Value) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
