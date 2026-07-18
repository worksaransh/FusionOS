using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Budgets.Events;

public sealed record BudgetCreated(Guid BudgetId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
