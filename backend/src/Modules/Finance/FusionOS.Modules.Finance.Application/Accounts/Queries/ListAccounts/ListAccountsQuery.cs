using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Queries.ListAccounts;

public sealed record ListAccountsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25) : IQuery<PagedResult<AccountDto>>;
