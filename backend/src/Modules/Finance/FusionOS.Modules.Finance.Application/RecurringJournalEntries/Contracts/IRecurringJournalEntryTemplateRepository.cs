namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;

public interface IRecurringJournalEntryTemplateRepository
{
    Task<Domain.RecurringJournalEntries.RecurringJournalEntryTemplate?> GetByIdAsync(Guid companyId, Guid recurringJournalEntryTemplateId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// When <paramref name="dueNow"/> is true, restricts to active templates whose NextDueDate
    /// has passed (NextDueDate &lt;= now) — backs ListRecurringJournalEntryTemplatesQuery's
    /// "templates ready to generate" filter. False/null returns every template for the company.
    /// </summary>
    Task<IReadOnlyList<Domain.RecurringJournalEntries.RecurringJournalEntryTemplate>> ListAsync(Guid companyId, bool? dueNow, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, bool? dueNow, CancellationToken cancellationToken = default);
}
