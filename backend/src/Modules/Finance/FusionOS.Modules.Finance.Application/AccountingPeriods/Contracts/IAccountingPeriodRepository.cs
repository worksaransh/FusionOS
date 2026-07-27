namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

public interface IAccountingPeriodRepository
{
    Task<Domain.AccountingPeriods.AccountingPeriod?> GetByIdAsync(Guid companyId, Guid accountingPeriodId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.AccountingPeriods.AccountingPeriod period, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.AccountingPeriods.AccountingPeriod>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Every period for this company, unpaged — backs CreateAccountingPeriodCommandHandler's overlap check, which needs to compare a candidate range against every existing period, not a page of them.</summary>
    Task<IReadOnlyList<Domain.AccountingPeriods.AccountingPeriod>> ListAllAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Backs CreateJournalEntryCommandHandler's closed-period guard: the single explicitly
    /// Closed period (if any) whose [PeriodStart, PeriodEnd] range covers <paramref name="date"/>
    /// for this company. Returns null both when no period at all covers the date (periods are
    /// opt-in — unrestricted, same as before this feature existed) and when a period covers it
    /// but is still Open — either way, posting is allowed.
    /// </summary>
    Task<Domain.AccountingPeriods.AccountingPeriod?> FindClosedPeriodCoveringDateAsync(Guid companyId, DateTimeOffset date, CancellationToken cancellationToken = default);
}
