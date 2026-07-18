using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class QuotationRepository : IQuotationRepository
{
    private readonly SalesDbContext _context;

    public QuotationRepository(SalesDbContext context) => _context = context;

    public Task<Quotation?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Quotations
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(Quotation quotation, CancellationToken cancellationToken = default) =>
        await _context.Quotations.AddAsync(quotation, cancellationToken);

    public async Task<IReadOnlyList<Quotation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Quotations
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.QuotationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Quotations.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
