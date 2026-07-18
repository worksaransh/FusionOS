using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.Recommendations;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Repositories;

public sealed class RecommendationRepository : IRecommendationRepository
{
    private readonly AiDbContext _context;

    public RecommendationRepository(AiDbContext context) => _context = context;

    public Task<Recommendation?> GetByIdAsync(Guid companyId, Guid recommendationId, CancellationToken cancellationToken = default) =>
        _context.Recommendations.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == recommendationId, cancellationToken);

    public async Task AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default) =>
        await _context.Recommendations.AddAsync(recommendation, cancellationToken);

    public async Task<IReadOnlyList<Recommendation>> ListAsync(Guid companyId, RecommendationStatus? status, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, status)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, RecommendationStatus? status, CancellationToken cancellationToken = default) =>
        Filtered(companyId, status).CountAsync(cancellationToken);

    private IQueryable<Recommendation> Filtered(Guid companyId, RecommendationStatus? status)
    {
        var query = _context.Recommendations.Where(r => r.CompanyId == companyId);
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        return query;
    }
}
