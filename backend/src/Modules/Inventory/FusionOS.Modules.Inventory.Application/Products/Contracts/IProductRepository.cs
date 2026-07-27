using FusionOS.Modules.Inventory.Domain.Products;

namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(Guid companyId, string sku, CancellationToken cancellationToken = default);
    // Barcode/QR support (2026-07-21) — the "scanner gun types code + Enter" lookup path.
    // Company-scoped like every other query here; barcode uniqueness is only enforced
    // per-company (see ProductConfiguration's filtered unique index), so the company must be
    // part of the lookup, not just the barcode value.
    Task<Product?> GetByBarcodeAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Added for the Attributes slice (see ProductVariantAttributeValue's doc
    /// comment): a direct, Include-independent existence check for a
    /// (ProductId, VariantId) pair. GetByIdAsync's query does not
    /// `.Include(p => p.Variants)` today, so relying on the returned
    /// aggregate's Variants collection to validate a variant id would
    /// silently see an empty list for any variant persisted in an earlier
    /// request. This method sidesteps that entirely by querying the
    /// underlying table directly rather than through the loaded navigation.
    /// </summary>
    Task<bool> VariantExistsAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);
}
