using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.SalesOrders.Events;

public sealed record SalesOrderCreated(Guid SalesOrderId, Guid CompanyId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
