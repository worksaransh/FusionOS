using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.SalesOrders.Events;

/// <summary>Matches "SalesOrderConfirmed.v1" in 03_SYSTEM_ARCHITECTURE.md §4.2's event catalog.</summary>
public sealed record SalesOrderConfirmed(Guid SalesOrderId, Guid CompanyId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
