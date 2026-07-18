using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListSuppliersQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SupplierDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.supplier.read" };
}
