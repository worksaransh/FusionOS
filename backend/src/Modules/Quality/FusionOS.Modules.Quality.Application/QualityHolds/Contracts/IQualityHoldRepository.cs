namespace FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

public interface IQualityHoldRepository
{
    Task<Domain.QualityHolds.QualityHold?> GetByIdAsync(Guid companyId, Guid qualityHoldId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.QualityHolds.QualityHold hold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.QualityHolds.QualityHold>> ListAsync(Guid companyId, string? entityType, Guid? entityId, bool? activeOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? entityType, Guid? entityId, bool? activeOnly, CancellationToken cancellationToken = default);
}
