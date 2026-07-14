using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Domain.Invoices;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly SalesDbContext _context;

    public InvoiceRepository(SalesDbContext context) => _context = context;

    public Task<Invoice?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Invoices.Include(i => i.Lines).FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == id, cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await _context.Invoices.AddAsync(invoice, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Invoices
            .Include(i => i.Lines)
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Invoices.CountAsync(i => i.CompanyId == companyId, cancellationToken);
}
