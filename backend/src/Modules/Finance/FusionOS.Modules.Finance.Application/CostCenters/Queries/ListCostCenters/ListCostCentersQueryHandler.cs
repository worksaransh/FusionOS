using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.CostCenters.Queries.ListCostCenters;

public sealed class ListCostCentersQueryHandler : IRequestHandler<ListCostCentersQuery, PagedResult<CostCenterDto>>
{
    private readonly ICostCenterRepository _repository;

    public ListCostCentersQueryHandler(ICostCenterRepository repository) => _repository = repository;

    public async Task<PagedResult<CostCenterDto>> Handle(ListCostCentersQuery request, CancellationToken cancellationToken)
    {
        var costCenters = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = costCenters.Select(CreateCostCenterCommandHandler.MapToDto).ToList();

        return new PagedResult<CostCenterDto>(dtos, request.Page, request.PageSize, total);
    }
}
