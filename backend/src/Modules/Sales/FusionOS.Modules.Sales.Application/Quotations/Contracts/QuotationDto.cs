namespace FusionOS.Modules.Sales.Application.Quotations.Contracts;

public sealed record QuotationLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record QuotationDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    DateTimeOffset QuotationDate,
    Guid? ConvertedSalesOrderId,
    decimal TotalAmount,
    IReadOnlyList<QuotationLineDto> Lines);
