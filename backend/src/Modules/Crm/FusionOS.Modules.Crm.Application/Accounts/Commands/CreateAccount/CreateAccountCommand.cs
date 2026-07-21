using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(Guid CompanyId, string Name, string? Industry, string? Website)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.account.create" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
