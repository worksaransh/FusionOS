using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Queries.ListAccounts;

public sealed record ListAccountsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AccountDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.account.read" };
}
