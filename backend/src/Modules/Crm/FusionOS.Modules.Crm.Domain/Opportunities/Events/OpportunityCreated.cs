using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Opportunities.Events;

/// <summary>Raised when an opportunity is opened from a qualified lead. No consumer today — a natural future hook for pipeline reporting.</summary>
public sealed record OpportunityCreated(Guid OpportunityId, Guid CompanyId, Guid LeadId, decimal EstimatedValue) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
