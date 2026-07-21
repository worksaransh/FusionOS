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

    // InvoiceLine is a plain FK-mapped entity (InvoiceConfiguration: HasMany(...).WithOne()
    // .HasForeignKey("InvoiceId")), not an EF owned type, so a SelectMany over the Lines
    // navigation translates to a single SQL join+SUM - same shape as
    // JournalEntryRepository.SumPostedAmountByAccountAsync (Finance). Fixed 2026-07-21:
    // the previous version loaded every invoice + all lines for the sales order into
    // memory and summed client-side.
    public Task<decimal> GetInvoicedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var lines =
            from invoice in _context.Invoices
            where invoice.CompanyId == companyId && invoice.SalesOrderId == salesOrderId
            from line in invoice.Lines
            where line.ProductId == productId
            select line;

        return lines.SumAsync(l => l.Quantity, cancellationToken);
    }

    // TotalAmount is a computed, EF-ignored property, so it can't be referenced inside a
    // translated query - but its constituent, LineTotal, is a real mapped column, so this
    // groups by SalesPersonId and sums LineTotal directly in a single grouped SQL query
    // (same shape as JournalEntryRepository.GetPostedBalancesByAccountInRangeAsync).
    // Fixed 2026-07-21: the previous version loaded every Issued invoice company-wide
    // with all lines into memory and grouped/summed client-side.
    public async Task<IReadOnlyList<(Guid SalesPersonId, decimal TotalInvoicedRevenue)>> GetIssuedInvoiceTotalsBySalesPersonAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await (
            from invoice in _context.Invoices
            where invoice.CompanyId == companyId
                  && invoice.Status == InvoiceStatus.Issued
                  && invoice.SalesPersonId != null
            from line in invoice.Lines
            group line by invoice.SalesPersonId into g
            select new { SalesPersonId = g.Key!.Value, TotalInvoicedRevenue = g.Sum(l => l.LineTotal) })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.SalesPersonId, g.TotalInvoicedRevenue)).ToList();
    }
}
