using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(Guid CompanyId, string Code, string Name, string AccountType, Guid? ParentAccountId)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.account.create" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
