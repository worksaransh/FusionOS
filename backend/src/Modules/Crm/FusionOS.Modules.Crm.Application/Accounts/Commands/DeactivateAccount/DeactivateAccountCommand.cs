using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.DeactivateAccount;

/// <summary>Soft-deactivate only — see Account.Deactivate(). Never a hard delete.</summary>
public sealed record DeactivateAccountCommand(Guid CompanyId, Guid AccountId)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.account.deactivate" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId => AccountId;
    public string Action => "Deactivated";
}
