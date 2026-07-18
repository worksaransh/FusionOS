using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;

public sealed record CreateBudgetLineCommand(Guid CompanyId, Guid BudgetId, Guid AccountId, Guid? CostCenterId, decimal BudgetedAmount, string? Notes)
    : ICommand<BudgetLineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.budget-line.create" };
    public string EntityType => nameof(Domain.BudgetLines.BudgetLine);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
