using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.FeatureFlags.Events;

public sealed record FeatureFlagCreated(Guid FeatureFlagId, Guid CompanyId, string Key) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
