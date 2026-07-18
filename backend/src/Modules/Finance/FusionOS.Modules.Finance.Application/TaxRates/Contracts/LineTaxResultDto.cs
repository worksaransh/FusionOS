namespace FusionOS.Modules.Finance.Application.TaxRates.Contracts;

/// <summary>
/// Result of CalculateLineTaxQuery — the tax computed for a single net line
/// amount against one TaxRate, returning which rate/percentage was actually
/// applied so callers (and this slice's frontend widget) can show their work
/// rather than a bare number. Deliberately mirrors ConversionResultDto's shape
/// (ExchangeRates.ConvertAmount): a pure lookup-and-multiply result, not a row.
/// </summary>
public sealed record LineTaxResultDto(
    decimal NetAmount,
    Guid TaxRateId,
    string TaxRateCode,
    decimal Percentage,
    decimal TaxAmount,
    decimal GrossAmount);
