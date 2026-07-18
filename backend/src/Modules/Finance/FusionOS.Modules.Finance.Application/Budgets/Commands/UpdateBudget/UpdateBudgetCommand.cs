using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.UpdateBudget;

public sealed record UpdateBudgetCommand(Guid CompanyId, Guid BudgetId, string Name, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : ICommand<BudgetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.budget.update" };
    public string EntityType => nameof(Domain.Budgets.Budget);
    public Guid EntityId => BudgetId;
    public string Action => "Updated";
}
