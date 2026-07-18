using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetById;

public sealed record GetBudgetByIdQuery(Guid CompanyId, Guid BudgetId)
    : IQuery<BudgetDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.budget.read" };
}
