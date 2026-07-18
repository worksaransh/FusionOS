using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Leads.Events;

/// <summary>Raised when a lead is captured. No consumer today — a natural future hook for lead-scoring/marketing automation.</summary>
public sealed record LeadCreated(Guid LeadId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
