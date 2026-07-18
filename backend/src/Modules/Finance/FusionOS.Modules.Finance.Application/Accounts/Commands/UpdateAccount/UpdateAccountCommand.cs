using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(Guid CompanyId, Guid AccountId, string Name, string AccountType, Guid? ParentAccountId)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.account.update" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId => AccountId;
    public string Action => "Updated";
}
