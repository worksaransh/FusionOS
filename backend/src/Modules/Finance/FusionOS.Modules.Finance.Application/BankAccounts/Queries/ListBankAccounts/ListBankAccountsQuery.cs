using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Queries.ListBankAccounts;

public sealed record ListBankAccountsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BankAccountDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.bank-account.read" };
}
