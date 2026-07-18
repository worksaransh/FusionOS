using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Discounts.Events;

public sealed record DiscountRuleCreated(Guid DiscountRuleId, Guid CompanyId, Guid ProductId, decimal MinQuantity, decimal DiscountPercentage) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
