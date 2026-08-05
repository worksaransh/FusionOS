namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;

public interface IIntercompanyEliminationRuleRepository
{
    Task<Domain.IntercompanyEliminationRules.IntercompanyEliminationRule?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.IntercompanyEliminationRules.IntercompanyEliminationRule rule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.IntercompanyEliminationRules.IntercompanyEliminationRule>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every active rule for this company, unpaged — backs
    /// GetConsolidatedTrialBalanceQueryHandler's elimination pass, which needs the
    /// whole active rule set to net out matching account-code pairs, not a page of it.
    /// Same "unpaged bulk fetch for a report" rationale as
    /// IAccountRepository.ListAllAsync/ICompanyRepository.ListByParentCompanyIdAsync.
    /// </summary>
    Task<IReadOnlyList<Domain.IntercompanyEliminationRules.IntercompanyEliminationRule>> ListActiveAsync(Guid companyId, CancellationToken cancellationToken = default);
}
