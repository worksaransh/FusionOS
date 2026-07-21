namespace FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

public interface IFeatureFlagRepository
{
    Task<bool> KeyExistsAsync(Guid companyId, string key, CancellationToken cancellationToken = default);
    Task<Domain.FeatureFlags.FeatureFlag?> GetByIdAsync(Guid companyId, Guid featureFlagId, CancellationToken cancellationToken = default);
    Task<Domain.FeatureFlags.FeatureFlag?> GetByKeyAsync(Guid companyId, string key, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.FeatureFlags.FeatureFlag featureFlag, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.FeatureFlags.FeatureFlag>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
