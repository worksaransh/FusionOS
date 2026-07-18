using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.BudgetLines.Events;

public sealed record BudgetLineCreated(Guid BudgetLineId, Guid CompanyId, Guid BudgetId, Guid AccountId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
