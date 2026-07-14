namespace FusionOS.Modules.Sales.Domain.SalesOrders;

public sealed record SalesOrderLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice);
