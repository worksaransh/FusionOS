using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using FusionOS.Modules.Inventory.Domain.Batches;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

/// <summary>
/// Uses DbContext.Set&lt;Batch&gt;() rather than a dedicated InventoryDbContext.Batches
/// DbSet property — the entity is still fully registered in the model via
/// BatchConfiguration (ApplyConfigurationsFromAssembly, InventoryDbContext.OnModelCreating),
/// this just avoids touching InventoryDbContext.cs directly (owned by a
/// parallel change). Behaves identically to a DbSet property; add one later
/// if it reads better once that file is free to edit.
/// </summary>
public sealed class BatchRepository : IBatchRepository
{
    private readonly InventoryDbContext _context;

    public BatchRepository(InventoryDbContext context) => _context = context;

    private DbSet<Batch> Batches => _context.Set<Batch>();

    public Task<Batch?> GetByIdAsync(Guid companyId, Guid batchId, CancellationToken cancellationToken = default) =>
        Batches.FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Id == batchId, cancellationToken);

    public Task<bool> BatchNumberExistsAsync(Guid companyId, Guid productId, string batchNumber, CancellationToken cancellationToken = default) =>
        Batches.AnyAsync(b => b.CompanyId == companyId && b.ProductId == productId && b.BatchNumber == batchNumber.Trim(), cancellationToken);

    public async Task AddAsync(Batch batch, CancellationToken cancellationToken = default) =>
        await Batches.AddAsync(batch, cancellationToken);

    public async Task<IReadOnlyList<Batch>> ListByProductAsync(Guid companyId, Guid productId, DateTimeOffset? expiringBefore, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, productId, expiringBefore)
            .OrderBy(b => b.ExpiryDate ?? DateTimeOffset.MaxValue)
            .ThenByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByProductAsync(Guid companyId, Guid productId, DateTimeOffset? expiringBefore, CancellationToken cancellationToken = default) =>
        Filtered(companyId, productId, expiringBefore).CountAsync(cancellationToken);

    private IQueryable<Batch> Filtered(Guid companyId, Guid productId, DateTimeOffset? expiringBefore)
    {
        var query = Batches.Where(b => b.CompanyId == companyId && b.ProductId == productId);
        if (expiringBefore.HasValue)
            query = query.Where(b => b.ExpiryDate != null && b.ExpiryDate < expiringBefore.Value);
        return query;
    }
}
