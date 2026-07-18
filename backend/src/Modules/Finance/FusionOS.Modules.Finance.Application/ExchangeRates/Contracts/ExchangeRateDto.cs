namespace FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

public sealed record ExchangeRateDto(
    Guid Id,
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal Rate,
    DateTimeOffset EffectiveDate,
    bool IsActive,
    DateTimeOffset CreatedAt);
