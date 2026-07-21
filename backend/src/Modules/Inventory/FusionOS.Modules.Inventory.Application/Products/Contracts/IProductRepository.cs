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
}
