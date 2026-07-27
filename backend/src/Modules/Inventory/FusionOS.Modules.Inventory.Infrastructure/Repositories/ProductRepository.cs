using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context) => _context = context;

    // Fixed 2026-07-21: this was missing Include(p => p.Variants) - every GetById/List
    // read silently returned an empty Variants collection regardless of how many
    // variants actually existed, since EF Core doesn't lazy-load by default and
    // ProductMapper.ToDto reads product.Variants directly. Found while building the
    // Attributes slice (VariantExistsAsync had to query around this same gap directly
    // rather than relying on a loaded collection); fixed here at the source instead.
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Products.Include(p => p.UnitOfMeasureConversions).Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> SkuExistsAsync(Guid companyId, string sku, CancellationToken cancellationToken = default) =>
        _context.Products.AnyAsync(p => p.CompanyId == companyId && p.Sku == sku.Trim().ToUpper(), cancellationToken);

    // Barcode/QR support (2026-07-21). Unlike Sku, Barcode isn't case-normalized on assignment
    // (see Product.AssignBarcode — a QR payload could legitimately be a case-sensitive string
    // such as a URL), so this is an exact match on the trimmed input, not a case-insensitive one.
    public Task<Product?> GetByBarcodeAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default) =>
        _context.Products.Include(p => p.UnitOfMeasureConversions).Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Barcode == barcode.Trim(), cancellationToken);

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

    // Added for the Attributes slice — see IProductRepository.VariantExistsAsync's
    // doc comment for why this queries the table directly (SelectMany over the
    // navigation translates to a SQL join) instead of relying on an Included/
    // loaded Variants collection.
    public Task<bool> VariantExistsAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default) =>
        _context.Products
            .Where(p => p.Id == productId)
            .SelectMany(p => p.Variants)
            .AnyAsync(v => v.Id == variantId, cancellationToken);

    // Matches on SKU or name — the two fields a picker's search box would reasonably type into.
    private IQueryable<Product> Filtered(Guid companyId, string? search)
    {
        var query = _context.Products.Include(p => p.UnitOfMeasureConversions).Include(p => p.Variants).Where(p => p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Sku, pattern) || EF.Functions.ILike(p.Name, pattern));
        }
        return query;
    }
}
