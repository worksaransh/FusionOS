namespace FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

public sealed record SalesOrderLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record SalesOrderDto(Guid Id, Guid CustomerId, string Status, DateTimeOffset OrderDate, decimal TotalAmount, IReadOnlyList<SalesOrderLineDto> Lines);
