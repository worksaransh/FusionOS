namespace FusionOS.Modules.Finance.Application.BudgetLines.Contracts;

/// <summary>Mirrors ICostCenterRepository's shape, scoped to a parent Budget instead of a company-wide list — ListByBudgetAsync/CountByBudgetAsync back both ListBudgetLinesQuery and GetBudgetVsActualQueryHandler's per-budget line lookup.</summary>
public interface IBudgetLineRepository
{
    Task<Domain.BudgetLines.BudgetLine?> GetByIdAsync(Guid companyId, Guid budgetLineId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.BudgetLines.BudgetLine budgetLine, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.BudgetLines.BudgetLine>> ListByBudgetAsync(Guid companyId, Guid budgetId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountByBudgetAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default);

    /// <summary>Unpaged — backs GetBudgetVsActualQueryHandler, which needs every line for the budget to compute the full vs-actual report in one pass, not one page of it.</summary>
    Task<IReadOnlyList<Domain.BudgetLines.BudgetLine>> ListAllByBudgetAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default);
}
