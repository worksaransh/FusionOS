using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.ListAccountingPeriods;

public sealed record ListAccountingPeriodsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AccountingPeriodDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.accounting-period.read" };
}
