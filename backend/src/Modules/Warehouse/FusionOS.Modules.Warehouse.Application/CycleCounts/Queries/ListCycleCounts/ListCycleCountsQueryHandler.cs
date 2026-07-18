using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.ListCycleCounts;

public sealed class ListCycleCountsQueryHandler : IRequestHandler<ListCycleCountsQuery, PagedResult<CycleCountDto>>
{
    private readonly ICycleCountRepository _repository;

    public ListCycleCountsQueryHandler(ICycleCountRepository repository) => _repository = repository;

    public async Task<PagedResult<CycleCountDto>> Handle(ListCycleCountsQuery request, CancellationToken cancellationToken)
    {
        var cycleCounts = await _repository.ListAsync(request.CompanyId, request.WarehouseId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.WarehouseId, cancellationToken);

        var dtos = cycleCounts.Select(StartCycleCountCommandHandler.Map).ToList();

        return new PagedResult<CycleCountDto>(dtos, request.Page, request.PageSize, total);
    }
}
