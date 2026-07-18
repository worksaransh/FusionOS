using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.Assets.Events;

/// <summary>Raised on Asset creation. No consumer this slice — same deliberate restraint as Quality's InspectionCreated.</summary>
public sealed record AssetCreated(Guid AssetId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
