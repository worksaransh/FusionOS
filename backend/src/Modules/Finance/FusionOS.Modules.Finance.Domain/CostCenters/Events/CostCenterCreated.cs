using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.CostCenters.Events;

public sealed record CostCenterCreated(Guid CostCenterId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
