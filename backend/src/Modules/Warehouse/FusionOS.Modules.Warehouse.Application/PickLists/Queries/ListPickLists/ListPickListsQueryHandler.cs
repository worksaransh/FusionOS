using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Queries.ListPickLists;

public sealed class ListPickListsQueryHandler : IRequestHandler<ListPickListsQuery, PagedResult<PickListDto>>
{
    private readonly IPickListRepository _repository;

    public ListPickListsQueryHandler(IPickListRepository repository) => _repository = repository;

    public async Task<PagedResult<PickListDto>> Handle(ListPickListsQuery request, CancellationToken cancellationToken)
    {
        var pickLists = await _repository.ListAsync(request.CompanyId, request.WarehouseId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.WarehouseId, cancellationToken);

        var dtos = pickLists.Select(PickListMapper.MapToDto).ToList();

        return new PagedResult<PickListDto>(dtos, request.Page, request.PageSize, total);
    }
}
