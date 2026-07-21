using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(Guid CompanyId, Guid AccountId, string Name, string? Industry, string? Website)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.account.update" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId => AccountId;
    public string Action => "Updated";
}
