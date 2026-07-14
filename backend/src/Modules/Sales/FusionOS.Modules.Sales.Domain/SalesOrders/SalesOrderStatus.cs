namespace FusionOS.Modules.Sales.Domain.SalesOrders;

/// <summary>Stored as text via EF value conversion — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum SalesOrderStatus
{
    Draft,
    Confirmed,
}
