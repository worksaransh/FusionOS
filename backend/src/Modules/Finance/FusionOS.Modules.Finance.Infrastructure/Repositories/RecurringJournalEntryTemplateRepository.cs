using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class RecurringJournalEntryTemplateRepository : IRecurringJournalEntryTemplateRepository
{
    private readonly FinanceDbContext _context;

    public RecurringJournalEntryTemplateRepository(FinanceDbContext context) => _context = context;

    public Task<RecurringJournalEntryTemplate?> GetByIdAsync(Guid companyId, Guid recurringJournalEntryTemplateId, CancellationToken cancellationToken = default) =>
        _context.RecurringJournalEntryTemplates.Include(t => t.Lines).FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == recurringJournalEntryTemplateId, cancellationToken);

    public async Task AddAsync(RecurringJournalEntryTemplate template, CancellationToken cancellationToken = default) =>
        await _context.RecurringJournalEntryTemplates.AddAsync(template, cancellationToken);

    public async Task<IReadOnlyList<RecurringJournalEntryTemplate>> ListAsync(Guid companyId, bool? dueNow, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, dueNow)
            .OrderBy(t => t.NextDueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, bool? dueNow, CancellationToken cancellationToken = default) =>
        Filtered(companyId, dueNow).CountAsync(cancellationToken);

    private IQueryable<RecurringJournalEntryTemplate> Filtered(Guid companyId, bool? dueNow)
    {
        var query = _context.RecurringJournalEntryTemplates.Include(t => t.Lines).Where(t => t.CompanyId == companyId);
        if (dueNow == true)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(t => t.IsActive && t.NextDueDate <= now);
        }
        return query;
    }
}
