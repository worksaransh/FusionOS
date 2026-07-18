using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Rfqs.Events;

public sealed record RfqSent(Guid RfqId, Guid CompanyId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
