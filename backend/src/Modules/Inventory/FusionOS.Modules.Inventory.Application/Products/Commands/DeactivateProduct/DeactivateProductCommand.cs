using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProduct;

/// <summary>
/// Soft-deactivation only — never deletes the row (08_API_STANDARDS.md /
/// 04_DATABASE_GUIDELINES.md). Requires the new "inventory.product.deactivate"
/// permission (not yet in PermissionCatalog.cs — must be added centrally).
/// </summary>
public sealed record DeactivateProductCommand(Guid CompanyId, Guid Id)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.deactivate" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
