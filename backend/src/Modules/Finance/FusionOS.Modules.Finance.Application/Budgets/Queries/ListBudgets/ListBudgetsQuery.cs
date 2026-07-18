using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.ListBudgets;

public sealed record ListBudgetsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BudgetDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.budget.read" };
}
