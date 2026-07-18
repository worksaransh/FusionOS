using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record GetCustomerBalanceQuery(Guid CompanyId, Guid CustomerId)
    : IQuery<CustomerBalanceDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.receivable.read" };
}
