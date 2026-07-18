using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.FixedAssets.Events;

public sealed record FixedAssetCreated(Guid FixedAssetId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
