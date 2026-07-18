namespace FusionOS.Modules.Sales.Domain.PriceLists;

public sealed record PriceListEntryInput(Guid ProductId, decimal UnitPrice);
