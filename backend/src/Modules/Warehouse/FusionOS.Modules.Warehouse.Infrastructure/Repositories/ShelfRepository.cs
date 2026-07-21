using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class ShelfRepository : IShelfRepository
{
    private readonly WarehouseDbContext _context;

    public ShelfRepository(WarehouseDbContext context) => _context = context;

    public Task<Shelf?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Shelves.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> RackExistsAsync(Guid companyId, Guid rackId, CancellationToken cancellationToken = default) =>
        _context.Racks.AnyAsync(r => r.CompanyId == companyId && r.Id == rackId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, Guid rackId, string code, CancellationToken cancellationToken = default) =>
        _context.Shelves.AnyAsync(s => s.CompanyId == companyId && s.RackId == rackId && s.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Shelf shelf, CancellationToken cancellationToken = default) =>
        await _context.Shelves.AddAsync(shelf, cancellationToken);

    public async Task<IReadOnlyList<Shelf>> ListAsync(Guid companyId, Guid rackId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Shelves
            .Where(s => s.CompanyId == companyId && s.RackId == rackId)
            .OrderBy(s => s.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid rackId, CancellationToken cancellationToken = default) =>
        _context.Shelves.CountAsync(s => s.CompanyId == companyId && s.RackId == rackId, cancellationToken);
}
