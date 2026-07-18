using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Recommendations.Queries.ListRecommendations;

public sealed class ListRecommendationsQueryHandler : IRequestHandler<ListRecommendationsQuery, PagedResult<RecommendationDto>>
{
    private readonly IRecommendationRepository _repository;

    public ListRecommendationsQueryHandler(IRecommendationRepository repository) => _repository = repository;

    public async Task<PagedResult<RecommendationDto>> Handle(ListRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var recommendations = await _repository.ListAsync(request.CompanyId, request.Status, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Status, cancellationToken);

        var dtos = recommendations.Select(RecommendationMapper.ToDto).ToList();

        return new PagedResult<RecommendationDto>(dtos, request.Page, request.PageSize, total);
    }
}
