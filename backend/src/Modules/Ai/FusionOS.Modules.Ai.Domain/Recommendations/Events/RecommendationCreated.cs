using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.Recommendations.Events;

public sealed record RecommendationCreated(Guid RecommendationId, Guid CompanyId, string Type, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
