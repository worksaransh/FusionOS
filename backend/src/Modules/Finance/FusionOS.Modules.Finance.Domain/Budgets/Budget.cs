using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.Budgets.Events;

namespace FusionOS.Modules.Finance.Domain.Budgets;

/// <summary>
/// M8f — Finance depth: budgeting, the sixth of the Phase M8 a–h sub-slices.
/// A named budget for a period (e.g. "FY2026 Operating Budget",
/// PeriodStart=2026-01-01, PeriodEnd=2026-12-31), broken into per-account
/// (optionally per-cost-center) <see cref="BudgetLines.BudgetLine"/> line
/// items — see that class's own doc comment for the child shape, and
/// GetBudgetVsActualQueryHandler's doc comment for how "actual" is computed
/// from posted JournalEntry data.
///
/// <b>Scope decision (Phase M8f, 2026-07-17), documented here the same way
/// every other M8 sub-slice's class doc comment documents its scope-out:</b>
/// this is master data (a named period plus its line items) and a read-only
/// actual-vs-budget query only. No version/revision history (a corrected
/// budget line is fixed in place via UpdateAmount, same "no versioning"
/// choice ExchangeRate.UpdateRate/TaxRate.UpdateDetails made), no approval
/// workflow (core's ApprovalRequest feature exists but is not wired to
/// budgets in this slice), and no automated variance-alerting engine —
/// GetBudgetVsActualQuery returns the numbers, it does not notify anyone or
/// flag anything as over/under. Full multi-year rolling budgets, budget
/// versions, and variance alerts are a distinct, separately-scoped,
/// materially larger future phase.
///
/// PeriodStart/PeriodEnd are <see cref="DateTimeOffset"/>, matching the date
/// type JournalEntry.EntryDate/ExchangeRate.EffectiveDate/
/// BankStatementLine.TransactionDate already use — not DateOnly, which
/// nothing in this codebase uses.
/// </summary>
public sealed class Budget : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Budget() { }

    public static Budget Create(Guid companyId, string name, DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Budget name is required.", nameof(name));
        if (periodEnd <= periodStart)
            throw new ArgumentException("Budget period end must be after period start.", nameof(periodEnd));

        var budget = new Budget
        {
            CompanyId = companyId,
            Name = name.Trim(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
        };

        budget.Raise(new BudgetCreated(budget.Id, companyId, budget.Name));
        return budget;
    }

    /// <summary>Updates the mutable master-data fields captured at Create time. There is no separate business key to keep immutable here (unlike Account/CostCenter's Code) — Name/PeriodStart/PeriodEnd are all correctable in place.</summary>
    public void UpdateDetails(string name, DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Budget name is required.", nameof(name));
        if (periodEnd <= periodStart)
            throw new ArgumentException("Budget period end must be after period start.", nameof(periodEnd));

        Name = name.Trim();
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public void Deactivate() => IsActive = false;
}
