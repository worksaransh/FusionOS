using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AddProductVariant;

/// <summary>Phase 1 closeout (2026-07-18). Reuses "inventory.product.update" rather than a new permission code — this is product master-data maintenance, same tier as AddUnitOfMeasureConversionCommand.</summary>
public sealed record AddProductVariantCommand(Guid CompanyId, Guid ProductId, string VariantSku, string Attributes)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => ProductId;
    public string Action => "VariantAdded";
}
