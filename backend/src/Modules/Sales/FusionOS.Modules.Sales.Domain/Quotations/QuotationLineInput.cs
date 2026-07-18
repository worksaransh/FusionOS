namespace FusionOS.Modules.Sales.Domain.Quotations;

public sealed record QuotationLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice);
