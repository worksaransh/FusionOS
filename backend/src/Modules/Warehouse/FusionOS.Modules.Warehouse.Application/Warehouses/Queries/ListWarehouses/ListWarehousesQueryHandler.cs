using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Queries.ListWarehouses;

public sealed class ListWarehousesQueryHandler : IRequestHandler<ListWarehousesQuery, PagedResult<WarehouseDto>>
{
    private readonly IWarehouseRepository _repository;

    public ListWarehousesQueryHandler(IWarehouseRepository repository) => _repository = repository;

    public async Task<PagedResult<WarehouseDto>> Handle(ListWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = warehouses
            .Select(w => new WarehouseDto(w.Id, w.Name, w.Code, w.Address, w.IsActive, w.CreatedAt))
            .ToList();

        return new PagedResult<WarehouseDto>(dtos, request.Page, request.PageSize, total);
    }
}
