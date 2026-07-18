using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class TaxJurisdictionRepository : ITaxJurisdictionRepository
{
    private readonly FinanceDbContext _context;

    public TaxJurisdictionRepository(FinanceDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.TaxJurisdictions.AnyAsync(j => j.CompanyId == companyId && j.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<TaxJurisdiction?> GetByIdAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default) =>
        _context.TaxJurisdictions.FirstOrDefaultAsync(j => j.CompanyId == companyId && j.Id == taxJurisdictionId, cancellationToken);

    public async Task AddAsync(TaxJurisdiction taxJurisdiction, CancellationToken cancellationToken = default) =>
        await _context.TaxJurisdictions.AddAsync(taxJurisdiction, cancellationToken);

    public async Task<IReadOnlyList<TaxJurisdiction>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(j => j.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<TaxJurisdiction> Filtered(Guid companyId, string? search)
    {
        var query = _context.TaxJurisdictions.Where(j => j.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(j => EF.Functions.ILike(j.Code, pattern) || EF.Functions.ILike(j.Name, pattern));
        }
        return query;
    }
}
