using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Queries.ListProducts;

/// <summary>
/// Search is optional and server-side (04_DATABASE_GUIDELINES.md / 08_API_STANDARDS.md) —
/// replaces the frontend's previous "fetch up to 200 rows, filter in JS"
/// EntityCombobox pattern, which does not scale past a couple hundred rows.
/// Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.
/// </summary>
public sealed record ListProductsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ProductDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.product.read" };
}
