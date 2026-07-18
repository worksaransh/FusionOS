namespace FusionOS.Modules.Finance.Application.Budgets.Contracts;

public sealed record BudgetDto(
    Guid Id,
    string Name,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    bool IsActive,
    DateTimeOffset CreatedAt);
