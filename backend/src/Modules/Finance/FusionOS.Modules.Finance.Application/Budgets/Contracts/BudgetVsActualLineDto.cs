namespace FusionOS.Modules.Finance.Application.Budgets.Contracts;

/// <summary>
/// One row of GetBudgetVsActualQuery's report: one BudgetLine's budgeted
/// amount next to the actual posted-JournalEntry total for the same
/// AccountId within the parent Budget's PeriodStart/PeriodEnd — see
/// GetBudgetVsActualQueryHandler's own doc comment for the cost-center
/// limitation (CostCenterId is echoed from the BudgetLine for display, but
/// ActualAmount is NOT filtered by it — JournalEntryLine has no
/// CostCenterId yet).
/// </summary>
public sealed record BudgetVsActualLineDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    Guid? CostCenterId,
    decimal BudgetedAmount,
    decimal ActualAmount,
    decimal VarianceAmount);
