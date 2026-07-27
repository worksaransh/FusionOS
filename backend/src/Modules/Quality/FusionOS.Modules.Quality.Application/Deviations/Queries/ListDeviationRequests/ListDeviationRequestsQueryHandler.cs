using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Deviations.Queries.ListDeviationRequests;

public sealed class ListDeviationRequestsQueryHandler : IRequestHandler<ListDeviationRequestsQuery, PagedResult<DeviationRequestDto>>
{
    private readonly IDeviationRequestRepository _repository;

    public ListDeviationRequestsQueryHandler(IDeviationRequestRepository repository) => _repository = repository;

    public async Task<PagedResult<DeviationRequestDto>> Handle(ListDeviationRequestsQuery request, CancellationToken cancellationToken)
    {
        var deviationRequests = await _repository.ListAsync(request.CompanyId, request.InspectionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.InspectionId, cancellationToken);

        var dtos = deviationRequests.Select(DeviationRequestMapper.ToDto).ToList();

        return new PagedResult<DeviationRequestDto>(dtos, request.Page, request.PageSize, total);
    }
}
