using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.GetAccountingPeriodById;

public sealed record GetAccountingPeriodByIdQuery(Guid CompanyId, Guid AccountingPeriodId) : IQuery<AccountingPeriodDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.accounting-period.read" };
}
