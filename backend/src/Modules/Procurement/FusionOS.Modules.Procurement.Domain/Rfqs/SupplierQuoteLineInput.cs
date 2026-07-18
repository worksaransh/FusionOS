namespace FusionOS.Modules.Procurement.Domain.Rfqs;

public sealed record SupplierQuoteLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice);
