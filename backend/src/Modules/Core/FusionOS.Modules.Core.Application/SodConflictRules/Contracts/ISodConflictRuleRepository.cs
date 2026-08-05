namespace FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

public interface ISodConflictRuleRepository
{
    Task<Domain.SodConflictRules.SodConflictRule?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SodConflictRules.SodConflictRule rule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.SodConflictRules.SodConflictRule>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> ConflictExistsAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default);

    /// <summary>
    /// True when an active rule for this company covers the given permission pair, in either
    /// order. The one method the cross-module ISegregationOfDutiesService implementation
    /// (see Services/SegregationOfDutiesService.cs) actually calls at enforcement time.
    /// </summary>
    Task<bool> HasActiveConflictAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default);
}
