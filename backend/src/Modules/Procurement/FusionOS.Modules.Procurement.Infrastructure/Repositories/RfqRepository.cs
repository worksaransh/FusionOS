using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class RfqRepository : IRfqRepository
{
    private readonly ProcurementDbContext _context;

    public RfqRepository(ProcurementDbContext context) => _context = context;

    public Task<RequestForQuotation?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Rfqs
            .Include(x => x.Lines)
            .Include(x => x.SupplierQuotes).ThenInclude(q => q.Lines)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(RequestForQuotation rfq, CancellationToken cancellationToken = default) =>
        await _context.Rfqs.AddAsync(rfq, cancellationToken);

    public async Task<IReadOnlyList<RequestForQuotation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Rfqs
            .Include(x => x.Lines)
            .Include(x => x.SupplierQuotes).ThenInclude(q => q.Lines)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.RfqDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Rfqs.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
