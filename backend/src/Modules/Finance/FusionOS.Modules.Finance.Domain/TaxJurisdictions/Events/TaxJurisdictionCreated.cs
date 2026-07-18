using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.TaxJurisdictions.Events;

public sealed record TaxJurisdictionCreated(Guid TaxJurisdictionId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
