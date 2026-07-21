using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid CompanyId, Guid AccountId) : IQuery<AccountDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.account.read" };
}
