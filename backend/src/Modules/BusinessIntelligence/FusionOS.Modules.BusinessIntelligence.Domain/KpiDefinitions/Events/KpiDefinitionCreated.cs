using FusionOS.SharedKernel;

namespace FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions.Events;

/// <summary>Raised on KpiDefinition creation. No consumer this slice — same deliberate restraint as Maintenance's AssetCreated.</summary>
public sealed record KpiDefinitionCreated(Guid KpiDefinitionId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
