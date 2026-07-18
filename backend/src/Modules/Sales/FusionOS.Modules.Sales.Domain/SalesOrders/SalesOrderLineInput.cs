namespace FusionOS.Modules.Sales.Domain.SalesOrders;

/// <summary>
/// DiscountPercentage defaults to 0 so every existing call site (Quotation
/// conversion, tests, etc.) that predates the pricing/discount engine
/// (docs/IMPLEMENTATION_PLAN.md Phase 10 item 10) keeps compiling unchanged.
/// </summary>
public sealed record SalesOrderLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal DiscountPercentage = 0m);
