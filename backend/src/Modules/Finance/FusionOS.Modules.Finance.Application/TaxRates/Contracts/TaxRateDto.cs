namespace FusionOS.Modules.Finance.Application.TaxRates.Contracts;

public sealed record TaxRateDto(Guid Id, Guid TaxJurisdictionId, string Code, string Name, decimal Percentage, bool IsActive, DateTimeOffset CreatedAt);
