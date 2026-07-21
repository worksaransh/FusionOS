using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Queries.GetProductByBarcode;

/// <summary>
/// Barcode/QR support (2026-07-21) — the real "a USB barcode scanner gun (which just types
/// characters + Enter, exactly like a keyboard) feeds a code into a search box and the product
/// resolves instantly" warehouse workflow. Same read-gating as GetProductByIdQuery: the existing
/// "inventory.product.read" permission, no new permission code — a barcode lookup is just an
/// alternate way to find a product you could already read by id/SKU. Returns null (not a 404
/// exception) when nothing matches, same as GetProductByIdQuery, so the controller can map it to
/// a plain 404 the way ProductsController.GetById already does.
/// </summary>
public sealed record GetProductByBarcodeQuery(Guid CompanyId, string Barcode)
    : IQuery<ProductDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.product.read" };
}
