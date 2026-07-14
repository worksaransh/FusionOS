using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseRepository(WarehouseDbContext context) => _context = context;

    public Task<Domain.Warehouses.Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Warehouses.AnyAsync(x => x.CompanyId == companyId && x.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Domain.Warehouses.Warehouse warehouse, CancellationToken cancellationToken = default) =>
        await _context.Warehouses.AddAsync(warehouse, cancellationToken);

    public async Task<IReadOnlyList<Domain.Warehouses.Warehouse>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Domain.Warehouses.Warehouse> Filtered(Guid companyId, string? search)
    {
        var query = _context.Warehouses.Where(x => x.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, pattern) || EF.Functions.ILike(x.Name, pattern));
        }
        return query;
    }
}
