using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.Recommendations.Events;

/// <summary>Raised once a user confirms a recommendation — the human-in-the-loop gate 12_AI_PLATFORM.md §5 requires before any AI output affects the transactional ledger. No consumer this slice — the natural future hook once a real model-produced recommendation type exists to act on.</summary>
public sealed record RecommendationAccepted(Guid RecommendationId, Guid CompanyId, string Type, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
