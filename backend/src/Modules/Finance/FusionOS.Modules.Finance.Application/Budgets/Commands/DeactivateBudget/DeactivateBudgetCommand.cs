using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.DeactivateBudget;

/// <summary>Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same convention as every other M8 sub-slice's Deactivate command. A deactivated budget's lines and vs-actual report remain queryable — this only flips IsActive.</summary>
public sealed record DeactivateBudgetCommand(Guid CompanyId, Guid BudgetId)
    : ICommand<BudgetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.budget.deactivate" };
    public string EntityType => nameof(Domain.Budgets.Budget);
    public Guid EntityId => BudgetId;
    public string Action => "Deactivated";
}
