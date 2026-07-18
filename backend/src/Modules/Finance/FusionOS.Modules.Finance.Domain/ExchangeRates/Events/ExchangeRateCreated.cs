using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.ExchangeRates.Events;

public sealed record ExchangeRateCreated(Guid ExchangeRateId, Guid CompanyId, string FromCurrencyCode, string ToCurrencyCode) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
