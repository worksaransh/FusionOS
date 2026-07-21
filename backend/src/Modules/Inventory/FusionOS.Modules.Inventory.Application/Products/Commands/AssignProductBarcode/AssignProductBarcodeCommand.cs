using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AssignProductBarcode;

/// <summary>
/// Barcode/QR support (2026-07-21). Reuses "inventory.product.update" rather than a new
/// permission code — assigning (or clearing, when Barcode is null) a barcode is product
/// master-data maintenance, same tier as UpdateProductCommand and
/// AddUnitOfMeasureConversionCommand.
/// </summary>
public sealed record AssignProductBarcodeCommand(Guid CompanyId, Guid ProductId, string? Barcode)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => ProductId;
    public string Action => "BarcodeAssigned";
}
