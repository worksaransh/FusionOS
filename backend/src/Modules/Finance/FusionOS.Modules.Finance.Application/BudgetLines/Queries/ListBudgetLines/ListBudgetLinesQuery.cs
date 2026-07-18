using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Queries.ListBudgetLines;

public sealed record ListBudgetLinesQuery(Guid CompanyId, Guid BudgetId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BudgetLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.budget-line.read" };
}
