using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Zones.Queries.GetZoneById;

public sealed class GetZoneByIdQueryHandler : IRequestHandler<GetZoneByIdQuery, ZoneDto?>
{
    private readonly IZoneRepository _repository;

    public GetZoneByIdQueryHandler(IZoneRepository repository) => _repository = repository;

    public async Task<ZoneDto?> Handle(GetZoneByIdQuery request, CancellationToken cancellationToken)
    {
        var zone = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (zone is null || zone.CompanyId != request.CompanyId)
            return null;

        return new ZoneDto(zone.Id, zone.WarehouseId, zone.Name, zone.Code, zone.IsActive, zone.CreatedAt);
    }
}
