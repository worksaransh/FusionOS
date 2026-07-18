namespace FusionOS.Modules.Sales.Application.PriceLists.Contracts;

public sealed record PriceListEntryDto(Guid Id, Guid ProductId, decimal UnitPrice);

public sealed record PriceListDto(Guid Id, string Name, bool IsActive, IReadOnlyList<PriceListEntryDto> Entries);
