using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.LockFiscalPeriod;

public sealed record LockFiscalPeriodCommand(Guid CompanyId, Guid AccountingPeriodId)
    : ICommand<AccountingPeriodDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.period.lock" };
    public string EntityType => nameof(Domain.AccountingPeriods.AccountingPeriod);
    public Guid EntityId => AccountingPeriodId;
    public string Action => "Locked";
}
