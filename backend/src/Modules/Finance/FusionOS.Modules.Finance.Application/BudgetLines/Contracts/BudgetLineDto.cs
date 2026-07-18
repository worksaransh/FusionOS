namespace FusionOS.Modules.Finance.Application.BudgetLines.Contracts;

public sealed record BudgetLineDto(
    Guid Id,
    Guid BudgetId,
    Guid AccountId,
    Guid? CostCenterId,
    decimal BudgetedAmount,
    string? Notes,
    DateTimeOffset CreatedAt);
