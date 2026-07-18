using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Queries.GetAccountById;

/// <summary>Tenant-scoped single-account lookup — wires up AccountsController's missing GetById action. Read-gated the same as ListAccountsQuery.</summary>
public sealed record GetAccountByIdQuery(Guid CompanyId, Guid AccountId)
    : IQuery<AccountDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.account.read" };
}
