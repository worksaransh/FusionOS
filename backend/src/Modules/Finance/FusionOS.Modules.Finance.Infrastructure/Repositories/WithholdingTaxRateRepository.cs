using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.WithholdingTaxRates;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class WithholdingTaxRateRepository : IWithholdingTaxRateRepository
{
    private readonly FinanceDbContext _context;

    public WithholdingTaxRateRepository(FinanceDbContext context) => _context = context;

    public Task<WithholdingTaxRate?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.WithholdingTaxRates.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == id, cancellationToken);

    public async Task AddAsync(WithholdingTaxRate rate, CancellationToken cancellationToken = default) =>
        await _context.WithholdingTaxRates.AddAsync(rate, cancellationToken);

    public async Task<IReadOnlyList<WithholdingTaxRate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.WithholdingTaxRates
            .Where(r => r.CompanyId == companyId)
            .OrderBy(r => r.SectionCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WithholdingTaxRates.CountAsync(r => r.CompanyId == companyId, cancellationToken);

    public Task<bool> SectionCodeExistsAsync(Guid companyId, string sectionCode, CancellationToken cancellationToken = default) =>
        _context.WithholdingTaxRates.AnyAsync(r => r.CompanyId == companyId && r.SectionCode == sectionCode.Trim(), cancellationToken);
}
