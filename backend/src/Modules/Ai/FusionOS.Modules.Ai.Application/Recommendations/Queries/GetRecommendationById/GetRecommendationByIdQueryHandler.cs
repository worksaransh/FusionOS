using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Recommendations.Queries.GetRecommendationById;

public sealed class GetRecommendationByIdQueryHandler : IRequestHandler<GetRecommendationByIdQuery, RecommendationDto>
{
    private readonly IRecommendationRepository _repository;

    public GetRecommendationByIdQueryHandler(IRecommendationRepository repository) => _repository = repository;

    public async Task<RecommendationDto> Handle(GetRecommendationByIdQuery request, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(request.CompanyId, request.RecommendationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recommendation '{request.RecommendationId}' was not found.");

        return RecommendationMapper.ToDto(recommendation);
    }
}
