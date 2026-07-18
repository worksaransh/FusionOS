using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.BudgetLines.Events;

namespace FusionOS.Modules.Finance.Domain.BudgetLines;

/// <summary>
/// M8f — Finance depth: budgeting. One line item within a
/// <see cref="Budgets.Budget"/>: a budgeted amount for one GL Account,
/// optionally scoped further to one CostCenter. Nests under Budget via
/// BudgetId the same way TaxRate nests under TaxJurisdiction — its own
/// top-level aggregate root with a real in-module FK to its parent, rather
/// than an owned child entity embedded in the Budget aggregate the way
/// JournalEntryLine is embedded in JournalEntry, since a budget line's own
/// lifecycle (corrected independently via UpdateAmount) doesn't need the
/// same single-transaction invariant (e.g. "must balance") a JournalEntry's
/// lines do.
///
/// AccountId is a real, same-module FK into Account, validated by the
/// command handler before this aggregate is created (same
/// domain-shape-only / handler-checks-existence split
/// CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId).
/// CostCenterId is optional and, if present, is validated the same way —
/// see CreateBudgetLineCommandHandler. Neither reference is enforced here.
///
/// No hard delete and no Remove/Delete method — matches this codebase's
/// established "soft-deactivate or correct-in-place, never a real delete"
/// convention (04_DATABASE_GUIDELINES.md); a mis-entered line is corrected
/// via UpdateAmount, the same way a mistyped ExchangeRate.Rate or
/// TaxRate.Percentage gets corrected in place rather than removed.
/// </summary>
public sealed class BudgetLine : TenantAggregateRoot
{
    public Guid BudgetId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public decimal BudgetedAmount { get; private set; }
    public string? Notes { get; private set; }

    private BudgetLine() { }

    /// <summary>A zero-budget line is legitimate (e.g. explicitly budgeting "no spend expected" for an account this period) — only a negative amount is rejected.</summary>
    public static BudgetLine Create(Guid companyId, Guid budgetId, Guid accountId, Guid? costCenterId, decimal budgetedAmount, string? notes)
    {
        if (budgetId == Guid.Empty)
            throw new ArgumentException("Budget id is required.", nameof(budgetId));
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account id is required.", nameof(accountId));
        if (budgetedAmount < 0)
            throw new ArgumentException("Budgeted amount cannot be negative.", nameof(budgetedAmount));

        var budgetLine = new BudgetLine
        {
            CompanyId = companyId,
            BudgetId = budgetId,
            AccountId = accountId,
            CostCenterId = costCenterId,
            BudgetedAmount = budgetedAmount,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };

        budgetLine.Raise(new BudgetLineCreated(budgetLine.Id, companyId, budgetId, accountId));
        return budgetLine;
    }

    /// <summary>Corrects the budgeted amount/notes in place. BudgetId/AccountId/CostCenterId are this line's identity and stay immutable after creation — a line for a different account/cost center is a new BudgetLine, not an edit of this one.</summary>
    public void UpdateAmount(decimal budgetedAmount, string? notes)
    {
        if (budgetedAmount < 0)
            throw new ArgumentException("Budgeted amount cannot be negative.", nameof(budgetedAmount));

        BudgetedAmount = budgetedAmount;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
