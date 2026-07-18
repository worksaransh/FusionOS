using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Queries.GetProductById;

/// <summary>
/// Wires the dead GetById stub in ProductsController (same documented gap as
/// CompaniesController). Tenant-scoped via the CompanyId property, which
/// TenantIsolationBehavior enforces against the caller's own company; the
/// handler additionally checks the loaded Product's CompanyId to guard
/// against a cross-tenant lookup by guessed id. Read-gated with the existing
/// "inventory.product.read" permission — no new permission code needed here.
/// </summary>
public sealed record GetProductByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<ProductDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.product.read" };
}
