using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.ReopenAccountingPeriod;

public sealed record ReopenAccountingPeriodCommand(Guid CompanyId, Guid AccountingPeriodId)
    : ICommand<AccountingPeriodDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.accounting-period.reopen" };
    public string EntityType => nameof(Domain.AccountingPeriods.AccountingPeriod);
    public Guid EntityId => AccountingPeriodId;
    public string Action => "Reopened";
}
