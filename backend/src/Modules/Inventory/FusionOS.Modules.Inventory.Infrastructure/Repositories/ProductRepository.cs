using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context) => _context = context;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> SkuExistsAsync(Guid companyId, string sku, CancellationToken cancellationToken = default) =>
        _context.Products.AnyAsync(p => p.CompanyId == companyId && p.Sku == sku.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await _context.Products.AddAsync(product, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    // Matches on SKU or name — the two fields a picker's search box would reasonably type into.
    private IQueryable<Product> Filtered(Guid companyId, string? search)
    {
        var query = _context.Products.Where(p => p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Sku, pattern) || EF.Functions.ILike(p.Name, pattern));
        }
        return query;
    }
}
