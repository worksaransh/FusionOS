using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.ListWorkCenters;

public sealed class ListWorkCentersQueryHandler : IRequestHandler<ListWorkCentersQuery, PagedResult<WorkCenterDto>>
{
    private readonly IWorkCenterRepository _repository;

    public ListWorkCentersQueryHandler(IWorkCenterRepository repository) => _repository = repository;

    public async Task<PagedResult<WorkCenterDto>> Handle(ListWorkCentersQuery request, CancellationToken cancellationToken)
    {
        var workCenters = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = workCenters.Select(WorkCenterMapper.ToDto).ToList();

        return new PagedResult<WorkCenterDto>(dtos, request.Page, request.PageSize, total);
    }
}
