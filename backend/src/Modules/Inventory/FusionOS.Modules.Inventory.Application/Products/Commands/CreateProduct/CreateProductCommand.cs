using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;

/// <summary>
/// Requires the "inventory.product.create" permission (07_SECURITY.md §2) —
/// unlike CreateCompany, this is not a bootstrap action, so it IS authorized.
/// </summary>
public sealed record CreateProductCommand(Guid CompanyId, string Sku, string Name, string UnitOfMeasure, string? Description)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.create" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
