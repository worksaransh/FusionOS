using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CloseAccountingPeriod;

public sealed record CloseAccountingPeriodCommand(Guid CompanyId, Guid AccountingPeriodId)
    : ICommand<AccountingPeriodDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.accounting-period.close" };
    public string EntityType => nameof(Domain.AccountingPeriods.AccountingPeriod);
    public Guid EntityId => AccountingPeriodId;
    public string Action => "Closed";
}
