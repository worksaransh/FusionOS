using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;

public sealed record CreateBudgetCommand(Guid CompanyId, string Name, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : ICommand<BudgetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.budget.create" };
    public string EntityType => nameof(Domain.Budgets.Budget);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
