namespace FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

/// <summary>Result of ConvertAmountQuery — the converted amount plus which rate/date the conversion actually used, so callers (and this slice's own frontend widget) can show their work rather than a bare number.</summary>
public sealed record ConversionResultDto(
    decimal OriginalAmount,
    string FromCurrencyCode,
    string ToCurrencyCode,
    decimal ConvertedAmount,
    decimal RateUsed,
    DateTimeOffset EffectiveDateOfRateUsed);
