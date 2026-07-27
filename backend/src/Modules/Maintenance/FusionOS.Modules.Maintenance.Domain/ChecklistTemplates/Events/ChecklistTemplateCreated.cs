using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.ChecklistTemplates.Events;

/// <summary>Raised on ChecklistTemplate creation. No consumer this slice — same deliberate restraint as AssetCreated.</summary>
public sealed record ChecklistTemplateCreated(Guid ChecklistTemplateId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
