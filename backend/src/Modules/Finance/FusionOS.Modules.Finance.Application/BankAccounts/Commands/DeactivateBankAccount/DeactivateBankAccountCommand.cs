using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.DeactivateBankAccount;

/// <summary>Soft-deactivate only — never a real delete (a bank account may already have statement lines recorded against it).</summary>
public sealed record DeactivateBankAccountCommand(Guid CompanyId, Guid BankAccountId)
    : ICommand<BankAccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-account.deactivate" };
    public string EntityType => nameof(Domain.BankAccounts.BankAccount);
    public Guid EntityId => BankAccountId;
    public string Action => "Deactivated";
}
