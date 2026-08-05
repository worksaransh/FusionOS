namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;

public sealed record WithholdingTaxRateDto(Guid Id, string SectionCode, string Description, decimal Percentage, bool IsActive);
