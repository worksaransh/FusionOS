namespace FusionOS.Modules.Sales.Application.Invoices.Contracts;

public sealed record InvoiceLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record InvoiceDto(
    Guid Id,
    Guid SalesOrderId,
    Guid CustomerId,
    string Status,
    DateTimeOffset InvoiceDate,
    decimal TotalAmount,
    IReadOnlyList<InvoiceLineDto> Lines);
