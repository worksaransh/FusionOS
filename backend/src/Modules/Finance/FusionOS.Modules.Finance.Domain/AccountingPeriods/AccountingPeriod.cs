using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.AccountingPeriods.Events;

namespace FusionOS.Modules.Finance.Domain.AccountingPeriods;

/// <summary>
/// Period control for the General Ledger (new — additive to Phase 2's JournalEntry, which
/// today has zero period control: entries can be posted to any date, forever, with no lock).
/// An AccountingPeriod is a named [PeriodStart, PeriodEnd] range per company, Open or Closed.
/// Non-overlapping-per-company is enforced by CreateAccountingPeriodCommandHandler (querying
/// existing periods before calling <see cref="Create"/>), not here — the same split
/// AccountingPeriod/Budget use for "is this a valid business key" checks (existence/uniqueness
/// needs a repository, so it lives in the handler; shape validation lives in the aggregate).
///
/// <b>Critical compatibility contract:</b> this is purely additive. Before this feature,
/// <see cref="JournalEntries.JournalEntry.Create"/> never checked any period at all — that
/// behaviour is unchanged for any company that never creates an AccountingPeriod, and for any
/// date that does not fall inside an explicitly-Closed period. Only an explicitly Closed period
/// blocks new postings (see CreateJournalEntryCommandHandler's own comment on the guard it
/// wires up). Periods are opt-in, not a mandatory calendar every company must pre-populate.
/// </summary>
public sealed class AccountingPeriod : TenantAggregateRoot
{
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public AccountingPeriodStatus Status { get; private set; }

    private AccountingPeriod() { }

    public static AccountingPeriod Create(Guid companyId, DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        if (periodEnd <= periodStart)
            throw new ArgumentException("Period end must be after period start.", nameof(periodEnd));

        var period = new AccountingPeriod
        {
            CompanyId = companyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = AccountingPeriodStatus.Open,
        };

        period.Raise(new AccountingPeriodCreated(period.Id, companyId, periodStart, periodEnd));
        return period;
    }

    /// <summary>True when <paramref name="date"/> falls inside [PeriodStart, PeriodEnd] inclusive. Used by CreateJournalEntryCommandHandler's closed-period guard.</summary>
    public bool Covers(DateTimeOffset date) => date >= PeriodStart && date <= PeriodEnd;

    /// <summary>
    /// Pure range-overlap comparison (no repository access) — used by
    /// CreateAccountingPeriodCommandHandler, which loads every existing period for the company
    /// and checks each one against the candidate [start, end] before calling <see cref="Create"/>.
    /// </summary>
    public bool OverlapsWith(DateTimeOffset otherStart, DateTimeOffset otherEnd) => PeriodStart <= otherEnd && otherStart <= PeriodEnd;

    public void Close()
    {
        if (Status == AccountingPeriodStatus.Closed)
            throw new InvalidOperationException("The accounting period is already closed.");

        Status = AccountingPeriodStatus.Closed;
        Raise(new AccountingPeriodClosed(Id, CompanyId, PeriodStart, PeriodEnd));
    }

    public void Reopen()
    {
        if (Status == AccountingPeriodStatus.Open)
            throw new InvalidOperationException("The accounting period is already open.");

        Status = AccountingPeriodStatus.Open;
        Raise(new AccountingPeriodReopened(Id, CompanyId, PeriodStart, PeriodEnd));
    }
}
