using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.UpdateBankAccount;

/// <summary>Update deliberately excludes Code (the immutable business key) and LinkedAccountId (a structural link, not a mutable detail) — see BankAccount.UpdateDetails's own doc comment.</summary>
public sealed record UpdateBankAccountCommand(Guid CompanyId, Guid BankAccountId, string Name, string? BankName, string? AccountNumberLast4)
    : ICommand<BankAccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-account.update" };
    public string EntityType => nameof(Domain.BankAccounts.BankAccount);
    public Guid EntityId => BankAccountId;
    public string Action => "Updated";
}
