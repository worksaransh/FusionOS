using FusionOS.Modules.Core.Domain.Companies;

namespace FusionOS.Modules.Core.Application.Companies.Contracts;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Every company whose ParentCompanyId equals <paramref name="parentCompanyId"/> —
    /// backs GetConsolidatedTrialBalanceQuery's subsidiary lookup (Phase 2.4). Unpaged,
    /// same rationale as IAccountRepository.ListAllAsync: consolidation needs the whole
    /// subsidiary set, not a page of it.
    /// </summary>
    Task<IReadOnlyList<Company>> ListByParentCompanyIdAsync(Guid parentCompanyId, CancellationToken cancellationToken = default);
}
