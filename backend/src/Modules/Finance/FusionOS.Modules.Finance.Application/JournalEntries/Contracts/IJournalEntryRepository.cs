namespace FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

public interface IJournalEntryRepository
{
    Task<Domain.JournalEntries.JournalEntry?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.JournalEntries.JournalEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.JournalEntries.JournalEntry>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
