using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Domain.CreditNotes;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class CreditNoteRepository : ICreditNoteRepository
{
    private readonly SalesDbContext _context;

    public CreditNoteRepository(SalesDbContext context) => _context = context;

    public Task<CreditNote?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.CreditNotes.Include(c => c.Lines).FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == id, cancellationToken);

    public async Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default) =>
        await _context.CreditNotes.AddAsync(creditNote, cancellationToken);

    public async Task<IReadOnlyList<CreditNote>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.CreditNotes
            .Include(c => c.Lines)
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.CreditNoteDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.CreditNotes.CountAsync(c => c.CompanyId == companyId, cancellationToken);

    // Loads matching credit notes with their lines and sums in memory, same
    // reasoning as InvoiceRepository.GetInvoicedQuantityAsync — safer to verify by
    // reading given no compiler is available in this environment.
    public async Task<decimal> GetCreditedQuantityAsync(Guid companyId, Guid invoiceId, Guid productId, CancellationToken cancellationToken = default)
    {
        var creditNotes = await _context.CreditNotes
            .Include(c => c.Lines)
            .Where(c => c.CompanyId == companyId && c.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        return creditNotes
            .SelectMany(c => c.Lines)
            .Where(l => l.ProductId == productId)
            .Sum(l => l.Quantity);
    }
}
