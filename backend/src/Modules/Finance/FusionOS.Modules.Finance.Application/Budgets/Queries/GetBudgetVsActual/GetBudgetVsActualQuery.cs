using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetVsActual;

public sealed record GetBudgetVsActualQuery(Guid CompanyId, Guid BudgetId)
    : IQuery<IReadOnlyList<BudgetVsActualLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.budget.read" };
}
