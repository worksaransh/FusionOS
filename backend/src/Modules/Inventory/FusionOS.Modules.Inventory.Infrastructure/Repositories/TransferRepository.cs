using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using FusionOS.Modules.Inventory.Domain.Transfers;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly InventoryDbContext _context;

    public TransferRepository(InventoryDbContext context) => _context = context;

    public Task<Transfer?> GetByIdAsync(Guid companyId, Guid transferId, CancellationToken cancellationToken = default) =>
        _context.Transfers.FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == transferId, cancellationToken);

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default) =>
        await _context.Transfers.AddAsync(transfer, cancellationToken);

    public async Task<IReadOnlyList<Transfer>> ListAsync(Guid companyId, Guid? productId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, productId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? productId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, productId).CountAsync(cancellationToken);

    private IQueryable<Transfer> Filtered(Guid companyId, Guid? productId)
    {
        var query = _context.Transfers.Where(t => t.CompanyId == companyId);
        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);
        return query;
    }
}
