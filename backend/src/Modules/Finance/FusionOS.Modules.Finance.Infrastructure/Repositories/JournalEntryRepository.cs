using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly FinanceDbContext _context;

    public JournalEntryRepository(FinanceDbContext context) => _context = context;

    public Task<JournalEntry?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.JournalEntries.Include(e => e.Lines).FirstOrDefaultAsync(e => e.CompanyId == companyId && e.Id == id, cancellationToken);

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default) =>
        await _context.JournalEntries.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<JournalEntry>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.JournalEntries
            .Include(e => e.Lines)
            .Where(e => e.CompanyId == companyId)
            .OrderByDescending(e => e.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.JournalEntries.CountAsync(e => e.CompanyId == companyId, cancellationToken);
}
