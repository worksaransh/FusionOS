using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.PriceLists.Events;

public sealed record PriceListCreated(Guid PriceListId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
