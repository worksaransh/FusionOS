using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Rfqs.Events;

public sealed record RfqCreated(Guid RfqId, Guid CompanyId, int LineCount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
