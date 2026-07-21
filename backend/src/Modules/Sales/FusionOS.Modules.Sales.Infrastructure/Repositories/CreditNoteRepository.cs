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

    // CreditNoteLine is a plain FK-mapped entity (CreditNoteConfiguration: HasMany(...)
    // .WithOne().HasForeignKey("CreditNoteId")), not an EF owned type, so a SelectMany
    // over the Lines navigation translates to a single SQL join+SUM - same shape as
    // JournalEntryRepository.SumPostedAmountByAccountAsync (Finance). Fixed 2026-07-21:
    // the previous version loaded every credit note + all lines for the invoice into
    // memory and summed client-side.
    public Task<decimal> GetCreditedQuantityAsync(Guid companyId, Guid invoiceId, Guid productId, CancellationToken cancellationToken = default)
    {
        var lines =
            from creditNote in _context.CreditNotes
            where creditNote.CompanyId == companyId && creditNote.InvoiceId == invoiceId
            from line in creditNote.Lines
            where line.ProductId == productId
            select line;

        return lines.SumAsync(l => l.Quantity, cancellationToken);
    }
}
