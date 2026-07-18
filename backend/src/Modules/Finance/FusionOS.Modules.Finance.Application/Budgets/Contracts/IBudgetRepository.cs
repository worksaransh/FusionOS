namespace FusionOS.Modules.Finance.Application.Budgets.Contracts;

/// <summary>Mirrors ICostCenterRepository's shape (GetById/AddAsync/ListAsync/CountAsync). ExistsAsync backs CreateBudgetLineCommandHandler's parent-existence check, same role IAccountRepository.ExistsAsync plays for CreateJournalEntryCommandHandler's line-level AccountId check.</summary>
public interface IBudgetRepository
{
    Task<Domain.Budgets.Budget?> GetByIdAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Budgets.Budget budget, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Budgets.Budget>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
