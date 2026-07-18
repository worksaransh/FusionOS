namespace FusionOS.Modules.Sales.Domain.Invoices;

public sealed record InvoiceLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice, Guid? TaxRateId = null, decimal TaxAmount = 0m);
