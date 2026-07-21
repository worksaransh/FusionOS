using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class BillOfMaterialsRepository : IBillOfMaterialsRepository
{
    private readonly ManufacturingDbContext _context;

    public BillOfMaterialsRepository(ManufacturingDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.BillsOfMaterials.AnyAsync(b => b.CompanyId == companyId && b.Code == code, cancellationToken);

    // .Include(x => x.Lines)/.Include(x => x.Operations) are required because both collections are backed by private fields.
    public Task<BillOfMaterials?> GetByIdAsync(Guid companyId, Guid billOfMaterialsId, CancellationToken cancellationToken = default) =>
        _context.BillsOfMaterials.Include(b => b.Lines).Include(b => b.Operations).FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Id == billOfMaterialsId, cancellationToken);

    public async Task AddAsync(BillOfMaterials billOfMaterials, CancellationToken cancellationToken = default) =>
        await _context.BillsOfMaterials.AddAsync(billOfMaterials, cancellationToken);

    public async Task<IReadOnlyList<BillOfMaterials>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .Include(b => b.Lines)
            .Include(b => b.Operations)
            .OrderBy(b => b.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<BillOfMaterials> Filtered(Guid companyId, string? search)
    {
        var query = _context.BillsOfMaterials.Where(b => b.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => EF.Functions.ILike(b.Code, $"%{term}%") || EF.Functions.ILike(b.Name, $"%{term}%"));
        }

        return query;
    }
}
