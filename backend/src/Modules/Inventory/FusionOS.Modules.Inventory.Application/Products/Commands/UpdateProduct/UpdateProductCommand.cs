using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Requires the new "inventory.product.update" permission (not yet in
/// PermissionCatalog.cs — must be added centrally). Sku is intentionally not
/// editable here — see Product.UpdateDetails.
/// </summary>
public sealed record UpdateProductCommand(Guid CompanyId, Guid Id, string Name, string UnitOfMeasure, string? Description)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
