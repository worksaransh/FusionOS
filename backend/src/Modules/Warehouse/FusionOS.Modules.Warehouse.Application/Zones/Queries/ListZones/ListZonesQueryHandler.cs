using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Zones.Queries.ListZones;

public sealed class ListZonesQueryHandler : IRequestHandler<ListZonesQuery, PagedResult<ZoneDto>>
{
    private readonly IZoneRepository _repository;

    public ListZonesQueryHandler(IZoneRepository repository) => _repository = repository;

    public async Task<PagedResult<ZoneDto>> Handle(ListZonesQuery request, CancellationToken cancellationToken)
    {
        var zones = await _repository.ListAsync(request.CompanyId, request.WarehouseId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.WarehouseId, cancellationToken);

        var dtos = zones.Select(z => new ZoneDto(z.Id, z.WarehouseId, z.Name, z.Code, z.IsActive, z.CreatedAt)).ToList();

        return new PagedResult<ZoneDto>(dtos, request.Page, request.PageSize, total);
    }
}
