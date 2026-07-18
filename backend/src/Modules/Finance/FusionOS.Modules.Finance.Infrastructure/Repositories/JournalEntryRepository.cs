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

    public async Task<decimal> SumPostedAmountByAccountAsync(Guid companyId, Guid accountId, DateTimeOffset dateFrom, DateTimeOffset dateTo, Guid? costCenterId = null, CancellationToken cancellationToken = default)
    {
        var lines =
            from entry in _context.JournalEntries
            where entry.CompanyId == companyId
                  && entry.Status == JournalEntryStatus.Posted
                  && entry.EntryDate >= dateFrom
                  && entry.EntryDate <= dateTo
            from line in entry.Lines
            where line.AccountId == accountId
                  && (costCenterId == null || line.CostCenterId == costCenterId)
            select line;

        var totalDebit = await lines.SumAsync(l => l.Debit, cancellationToken);
        var totalCredit = await lines.SumAsync(l => l.Credit, cancellationToken);
        return totalDebit - totalCredit;
    }

    public async Task<IReadOnlyList<JournalEntry>> FindPostedByAmountWithinDateRangeAsync(Guid companyId, decimal amountMagnitude, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken = default)
    {
        // The date window (typically +/-3 days) keeps this set small, so filtering the
        // balanced magnitude (a computed Sum over Lines) in memory after the DB-side
        // date/status filter is cheap and avoids translating the aggregate's computed
        // TotalDebit property into SQL.
        var windowed = await _context.JournalEntries
            .Include(e => e.Lines)
            .Where(e => e.CompanyId == companyId
                        && e.Status == JournalEntryStatus.Posted
                        && e.EntryDate >= dateFrom
                        && e.EntryDate <= dateTo)
            .ToListAsync(cancellationToken);

        return windowed.Where(e => e.TotalDebit == amountMagnitude).ToList();
    }

    public async Task<IReadOnlyList<(Guid AccountId, decimal TotalDebit, decimal TotalCredit)>> GetPostedBalancesByAccountAsOfAsync(Guid companyId, DateTimeOffset asOfDate, CancellationToken cancellationToken = default)
    {
        var grouped = await (
            from entry in _context.JournalEntries
            where entry.CompanyId == companyId
                  && entry.Status == JournalEntryStatus.Posted
                  && entry.EntryDate <= asOfDate
            from line in entry.Lines
            group line by line.AccountId into g
            select new { AccountId = g.Key, TotalDebit = g.Sum(l => l.Debit), TotalCredit = g.Sum(l => l.Credit) })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.AccountId, g.TotalDebit, g.TotalCredit)).ToList();
    }
}
