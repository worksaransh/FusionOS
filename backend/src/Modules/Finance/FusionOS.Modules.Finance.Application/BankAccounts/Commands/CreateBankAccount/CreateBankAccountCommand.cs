using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;

public sealed record CreateBankAccountCommand(Guid CompanyId, string Code, string Name, Guid LinkedAccountId, string? BankName, string? AccountNumberLast4)
    : ICommand<BankAccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-account.create" };
    public string EntityType => nameof(Domain.BankAccounts.BankAccount);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
