using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Customers.Events;

public sealed record CustomerCreated(Guid CustomerId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
