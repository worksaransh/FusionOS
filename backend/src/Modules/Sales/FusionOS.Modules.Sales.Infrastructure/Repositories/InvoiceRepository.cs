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

    // Loads matching invoices with their lines and sums in memory rather than
    // relying on EF translating a SelectMany over InvoiceLine (a private-field-backed
    // owned collection) directly into SQL - safer to verify by reading given no
    // compiler is available in this environment (2026-07-14 coverage-audit follow-up).
    public async Task<decimal> GetInvoicedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Lines)
            .Where(i => i.CompanyId == companyId && i.SalesOrderId == salesOrderId)
            .ToListAsync(cancellationToken);

        return invoices
            .SelectMany(i => i.Lines)
            .Where(l => l.ProductId == productId)
            .Sum(l => l.Quantity);
    }

    // TotalAmount is a computed, EF-ignored property (summed in memory from the
    // Lines navigation, same as GetInvoicedQuantityAsync above) - it cannot be
    // referenced inside a query EF would try to translate to SQL, so this loads
    // matching invoices with their lines first and groups/sums in memory.
    public async Task<IReadOnlyList<(Guid SalesPersonId, decimal TotalInvoicedRevenue)>> GetIssuedInvoiceTotalsBySalesPersonAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Lines)
            .Where(i => i.CompanyId == companyId && i.Status == InvoiceStatus.Issued && i.SalesPersonId != null)
            .ToListAsync(cancellationToken);

        return invoices
            .GroupBy(i => i.SalesPersonId!.Value)
            .Select(g => (SalesPersonId: g.Key, TotalInvoicedRevenue: g.Sum(i => i.TotalAmount)))
            .ToList();
    }
}
