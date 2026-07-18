using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.UpdateBudgetLineAmount;

/// <summary>Covers BudgetedAmount/Notes only — BudgetId/AccountId/CostCenterId are this line's identity and stay immutable (see BudgetLine.UpdateAmount's own doc comment). A line for a different account/cost center is a new CreateBudgetLineCommand, not an edit of this one.</summary>
public sealed record UpdateBudgetLineAmountCommand(Guid CompanyId, Guid BudgetLineId, decimal BudgetedAmount, string? Notes)
    : ICommand<BudgetLineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.budget-line.update" };
    public string EntityType => nameof(Domain.BudgetLines.BudgetLine);
    public Guid EntityId => BudgetLineId;
    public string Action => "Updated";
}
