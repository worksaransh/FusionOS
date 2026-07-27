using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Queries.ListInspectionPlans;

public sealed class ListInspectionPlansQueryHandler : IRequestHandler<ListInspectionPlansQuery, PagedResult<InspectionPlanDto>>
{
    private readonly IInspectionPlanRepository _repository;

    public ListInspectionPlansQueryHandler(IInspectionPlanRepository repository) => _repository = repository;

    public async Task<PagedResult<InspectionPlanDto>> Handle(ListInspectionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _repository.ListAsync(request.CompanyId, request.InspectionType, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.InspectionType, cancellationToken);

        var dtos = plans.Select(InspectionPlanMapper.ToDto).ToList();

        return new PagedResult<InspectionPlanDto>(dtos, request.Page, request.PageSize, total);
    }
}
