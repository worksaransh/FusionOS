using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Queries.ListQualityHolds;

public sealed class ListQualityHoldsQueryHandler : IRequestHandler<ListQualityHoldsQuery, PagedResult<QualityHoldDto>>
{
    private readonly IQualityHoldRepository _repository;

    public ListQualityHoldsQueryHandler(IQualityHoldRepository repository) => _repository = repository;

    public async Task<PagedResult<QualityHoldDto>> Handle(ListQualityHoldsQuery request, CancellationToken cancellationToken)
    {
        var holds = await _repository.ListAsync(request.CompanyId, request.EntityType, request.EntityId, request.ActiveOnly, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.EntityType, request.EntityId, request.ActiveOnly, cancellationToken);

        var dtos = holds.Select(QualityHoldMapper.ToDto).ToList();

        return new PagedResult<QualityHoldDto>(dtos, request.Page, request.PageSize, total);
    }
}
