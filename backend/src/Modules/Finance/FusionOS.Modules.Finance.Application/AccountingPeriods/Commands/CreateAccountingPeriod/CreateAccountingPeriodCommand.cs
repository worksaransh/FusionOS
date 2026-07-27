using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CreateAccountingPeriod;

public sealed record CreateAccountingPeriodCommand(Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : ICommand<AccountingPeriodDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.accounting-period.create" };
    public string EntityType => nameof(Domain.AccountingPeriods.AccountingPeriod);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
