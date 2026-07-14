using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly ProcurementDbContext _context;

    public SupplierRepository(ProcurementDbContext context) => _context = context;

    public Task<Domain.Suppliers.Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default) =>
        _context.Suppliers.AnyAsync(x => x.CompanyId == companyId && x.Id == supplierId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Suppliers.AnyAsync(x => x.CompanyId == companyId && x.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Domain.Suppliers.Supplier supplier, CancellationToken cancellationToken = default) =>
        await _context.Suppliers.AddAsync(supplier, cancellationToken);

    public async Task<IReadOnlyList<Domain.Suppliers.Supplier>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Domain.Suppliers.Supplier> Filtered(Guid companyId, string? search)
    {
        var query = _context.Suppliers.Where(x => x.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, pattern) || EF.Functions.ILike(x.Name, pattern));
        }
        return query;
    }
}
