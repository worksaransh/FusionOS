using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.DeactivateAccount;

/// <summary>Soft-deactivate only — never a real delete (an Account may already be referenced by posted journal entries).</summary>
public sealed record DeactivateAccountCommand(Guid CompanyId, Guid AccountId)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.account.deactivate" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId => AccountId;
    public string Action => "Deactivated";
}
