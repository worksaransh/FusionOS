using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Payables.Contracts;

namespace FusionOS.Modules.Finance.Application.Payables.Queries.GetSupplierBalance;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record GetSupplierBalanceQuery(Guid CompanyId, Guid SupplierId)
    : IQuery<SupplierBalanceDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.payable.read" };
}
