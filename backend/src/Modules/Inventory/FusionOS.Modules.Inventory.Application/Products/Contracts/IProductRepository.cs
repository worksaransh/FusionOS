using FusionOS.Modules.Inventory.Domain.Products;

namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(Guid companyId, string sku, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
