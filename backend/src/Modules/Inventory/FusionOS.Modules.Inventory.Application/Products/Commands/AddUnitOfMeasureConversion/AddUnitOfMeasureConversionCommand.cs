using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AddUnitOfMeasureConversion;

/// <summary>M9 remaining — Multi-UOM (2026-07-16). Reuses "inventory.product.update" rather than a new permission code — this is product master-data maintenance, same tier as UpdateProductCommand.</summary>
public sealed record AddUnitOfMeasureConversionCommand(Guid CompanyId, Guid ProductId, string AlternateUnitOfMeasure, decimal ConversionFactor)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => ProductId;
    public string Action => "UnitOfMeasureConversionAdded";
}
