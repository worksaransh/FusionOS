using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.TaxRates.Events;

public sealed record TaxRateCreated(Guid TaxRateId, Guid CompanyId, Guid TaxJurisdictionId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
