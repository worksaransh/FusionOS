using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Queries.GetBankAccountById;

public sealed record GetBankAccountByIdQuery(Guid CompanyId, Guid BankAccountId)
    : IQuery<BankAccountDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.bank-account.read" };
}
