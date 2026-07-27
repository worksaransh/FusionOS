using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Returns;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class SalesReturnRepository : ISalesReturnRepository
{
    private readonly SalesDbContext _context;

    public SalesReturnRepository(SalesDbContext context) => _context = context;

    public Task<SalesReturn?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.SalesReturns.Include(r => r.Lines).FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == id, cancellationToken);

    public async Task AddAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default) =>
        await _context.SalesReturns.AddAsync(salesReturn, cancellationToken);

    public async Task<IReadOnlyList<SalesReturn>> ListAsync(Guid companyId, int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SalesReturns.Include(r => r.Lines).Where(r => r.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SalesReturnStatus>(status, true, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        return await query
            .OrderByDescending(r => r.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid companyId, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SalesReturns.Where(r => r.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SalesReturnStatus>(status, true, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SalesReturn>> ListByInvoiceAsync(Guid companyId, Guid invoiceId, CancellationToken cancellationToken = default) =>
        await _context.SalesReturns
            .Include(r => r.Lines)
            .Where(r => r.CompanyId == companyId && r.InvoiceId == invoiceId)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(cancellationToken);

    // SalesReturnLine is a plain FK-mapped entity (SalesReturnConfiguration: HasMany(...)
    // .WithOne().HasForeignKey("SalesReturnId")), not an EF owned type, so a SelectMany
    // over the Lines navigation translates to a single SQL join+SUM - same shape as
    // CreditNoteRepository.GetCreditedQuantityAsync. Rejected returns are excluded: a
    // rejected return never consumed any of the invoice's returnable quantity.
    public Task<decimal> GetReturnedQuantityAsync(Guid companyId, Guid invoiceId, Guid productId, CancellationToken cancellationToken = default)
    {
        var lines =
            from salesReturn in _context.SalesReturns
            where salesReturn.CompanyId == companyId && salesReturn.InvoiceId == invoiceId && salesReturn.Status != SalesReturnStatus.Rejected
            from line in salesReturn.Lines
            where line.ProductId == productId
            select line;

        return lines.SumAsync(l => l.Quantity, cancellationToken);
    }
}
