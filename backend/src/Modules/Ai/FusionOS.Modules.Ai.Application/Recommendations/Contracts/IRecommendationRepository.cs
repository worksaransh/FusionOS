using FusionOS.Modules.Ai.Domain.Recommendations;

namespace FusionOS.Modules.Ai.Application.Recommendations.Contracts;

public interface IRecommendationRepository
{
    Task<Domain.Recommendations.Recommendation?> GetByIdAsync(Guid companyId, Guid recommendationId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Recommendations.Recommendation recommendation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Recommendations.Recommendation>> ListAsync(Guid companyId, RecommendationStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, RecommendationStatus? status, CancellationToken cancellationToken = default);
}
