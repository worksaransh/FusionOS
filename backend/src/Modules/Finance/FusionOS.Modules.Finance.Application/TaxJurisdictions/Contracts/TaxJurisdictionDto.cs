namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

public sealed record TaxJurisdictionDto(Guid Id, string Code, string Name, bool IsActive, DateTimeOffset CreatedAt);
