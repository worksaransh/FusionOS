using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class TaxRateRepository : ITaxRateRepository
{
    private readonly FinanceDbContext _context;

    public TaxRateRepository(FinanceDbContext context) => _context = context;

    public Task<TaxRate?> GetByIdAsync(Guid companyId, Guid taxRateId, CancellationToken cancellationToken = default) =>
        _context.TaxRates.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == taxRateId, cancellationToken);

    public Task<bool> TaxJurisdictionExistsAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default) =>
        _context.TaxJurisdictions.AnyAsync(j => j.CompanyId == companyId && j.Id == taxJurisdictionId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, Guid taxJurisdictionId, string code, CancellationToken cancellationToken = default) =>
        _context.TaxRates.AnyAsync(r => r.CompanyId == companyId && r.TaxJurisdictionId == taxJurisdictionId && r.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(TaxRate taxRate, CancellationToken cancellationToken = default) =>
        await _context.TaxRates.AddAsync(taxRate, cancellationToken);

    public async Task<IReadOnlyList<TaxRate>> ListAsync(Guid companyId, Guid taxJurisdictionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.TaxRates
            .Where(r => r.CompanyId == companyId && r.TaxJurisdictionId == taxJurisdictionId)
            .OrderBy(r => r.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default) =>
        _context.TaxRates.CountAsync(r => r.CompanyId == companyId && r.TaxJurisdictionId == taxJurisdictionId, cancellationToken);
}
